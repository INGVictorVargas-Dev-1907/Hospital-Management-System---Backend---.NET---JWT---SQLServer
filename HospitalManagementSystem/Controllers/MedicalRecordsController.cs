using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService, ILogger<MedicalRecordsController> logger)
        {
            _medicalRecordService = medicalRecordService;
            _logger = logger;
        }

        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetPatientMedicalHistory(Guid patientId)
        {
            try
            {
                // Verificar permisos - paciente solo puede ver su propio historial
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(patientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var medicalRecords = await _medicalRecordService.GetPatientMedicalHistoryAsync(patientId);
                return Ok(medicalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial médico del paciente {PatientId}", patientId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetAllMedicalRecords()
        {
            try
            {
                var medicalRecords = await _medicalRecordService.GetAllMedicalRecordsAsync();
                return Ok(medicalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los registros médicos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalRecordDto>> GetMedicalRecordById(Guid id)
        {
            try
            {
                var medicalRecord = await _medicalRecordService.GetMedicalRecordByIdAsync(id);

                if (medicalRecord == null)
                {
                    return NotFound(new { message = "Registro médico no encontrado" });
                }

                // Verificar permisos
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(medicalRecord.PatientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                return Ok(medicalRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registro médico {MedicalRecordId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MedicalRecordDto>> CreateMedicalRecord(CreateMedicalRecordDto createMedicalRecordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var medicalRecord = await _medicalRecordService.CreateMedicalRecordAsync(createMedicalRecordDto);
                return CreatedAtAction(nameof(GetMedicalRecordById), new { id = medicalRecord.Id }, medicalRecord);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro médico");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("patient/{patientId}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GenerateMedicalHistoryPdf(Guid patientId, [FromBody] MedicalHistoryPdfRequestDto? request = null)
        {
            try
            {
                // Verificar permisos - paciente solo puede generar su propio historial
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(patientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                DateTime? startDate = request?.StartDate;
                DateTime? endDate = request?.EndDate;

                var pdfBytes = await _medicalRecordService.GenerateMedicalHistoryPdfAsync(patientId, startDate, endDate);

                // Obtener información del paciente para el nombre del archivo
                var patientService2 = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                var patient = await patientService2.GetPatientByIdAsync(patientId);

                var fileName = $"Historial_Medico_{patient?.FirstName}_{patient?.LastName}_{DateTime.Now:yyyyMMdd}.pdf";

                _logger.LogInformation("PDF generado exitosamente para paciente {PatientId}", patientId);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del historial médico para paciente {PatientId}", patientId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("patient/{patientId}/pdf/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadMedicalHistoryPdf(Guid patientId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Verificar permisos - paciente solo puede descargar su propio historial
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(patientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var pdfBytes = await _medicalRecordService.GenerateMedicalHistoryPdfAsync(patientId, startDate, endDate);

                // Obtener información del paciente para el nombre del archivo
                var patientService2 = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                var patient = await patientService2.GetPatientByIdAsync(patientId);

                var fileName = $"Historial_Medico_{patient?.FirstName}_{patient?.LastName}_{DateTime.Now:yyyyMMdd}.pdf";

                _logger.LogInformation("PDF descargado exitosamente para paciente {PatientId}", patientId);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar PDF del historial médico para paciente {PatientId}", patientId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
