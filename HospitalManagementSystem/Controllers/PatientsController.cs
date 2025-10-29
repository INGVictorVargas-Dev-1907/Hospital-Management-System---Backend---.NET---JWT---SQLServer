using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetAllPatients()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pacientes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PatientDto>> GetPatientById(Guid id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);

                if (patient == null)
                {
                    return NotFound(new { message = "Paciente no encontrado" });
                }

                // Verificar que el usuario tenga permisos para ver este paciente
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Doctor" && userRole != "Employee")
                {
                    // Si es paciente, solo puede ver su propia información
                    var patientUserId = await _patientService.GetPatientUserIdAsync(id);
                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente con ID: {PatientId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PatientDto>> CreatePatient(CreatePatientDto createPatientDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var patient = await _patientService.CreatePatientAsync(createPatientDto);
                return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear paciente");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PatientDto>> UpdatePatient(Guid id, UpdatePatientDto updatePatientDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar permisos
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && userRole != "Employee")
                {
                    var patientUserId = await _patientService.GetPatientUserIdAsync(id);
                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var patient = await _patientService.UpdatePatientAsync(id, updatePatientDto);

                if (patient == null)
                {
                    return NotFound(new { message = "Paciente no encontrado" });
                }

                return Ok(patient);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar paciente: {PatientId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeletePatient(Guid id)
        {
            try
            {
                var result = await _patientService.DeletePatientAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Paciente no encontrado" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paciente: {PatientId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin,Doctor,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PatientDto>>> SearchPatients([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return BadRequest(new { message = "El término de búsqueda debe tener al menos 2 caracteres" });
                }

                var patients = await _patientService.SearchPatientsAsync(term);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar pacientes con término: {SearchTerm}", term);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
