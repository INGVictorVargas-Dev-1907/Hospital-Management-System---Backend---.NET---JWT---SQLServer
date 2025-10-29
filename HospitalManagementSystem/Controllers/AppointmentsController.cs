using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las citas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByPatient(Guid patientId)
        {
            try
            {
                // Verificar permisos - paciente solo puede ver sus propias citas
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    // Obtener el userId del paciente para verificar permisos
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(patientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del paciente {PatientId}", patientId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Admin,Doctor,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByDoctor(Guid doctorId)
        {
            try
            {
                // Verificar que el doctor solo pueda ver sus propias citas
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Doctor")
                {
                    // Aquí necesitaríamos un servicio para obtener el doctorId del usuario actual
                    // Por ahora, asumimos que el doctor solo puede ver sus propias citas
                    // En una implementación real, necesitaríamos mapear UserId a DoctorId
                }

                var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del doctor {DoctorId}", doctorId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Employee,Doctor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByStatus(string status)
        {
            try
            {
                var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest(new { message = "Estado de cita no válido" });
                }

                var appointments = await _appointmentService.GetAppointmentsByStatusAsync(status);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas por estado: {Status}", status);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,Patient")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar permisos - paciente solo puede crear citas para sí mismo
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(createAppointmentDto.PatientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                var appointment = await _appointmentService.CreateAppointmentAsync(createAppointmentDto);
                return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cita");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AppointmentDto>> GetAppointmentById(Guid id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

                if (appointment == null)
                {
                    return NotFound(new { message = "Cita no encontrada" });
                }

                // Verificar permisos
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole == "Patient")
                {
                    var patientService = HttpContext.RequestServices.GetRequiredService<IPatientService>();
                    var patientUserId = await patientService.GetPatientUserIdAsync(appointment.PatientId);

                    if (patientUserId.ToString() != currentUserId)
                    {
                        return Forbid();
                    }
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cita {AppointmentId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAppointmentStatus(Guid id, [FromBody] string status)
        {
            try
            {
                var result = await _appointmentService.UpdateAppointmentStatusAsync(id, status);

                if (!result)
                {
                    return NotFound(new { message = "Cita no encontrada" });
                }

                return Ok(new { message = "Estado de cita actualizado exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de cita {AppointmentId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelAppointment(Guid id)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "Cita no encontrada" });
                }

                return Ok(new { message = "Cita cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cita {AppointmentId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
