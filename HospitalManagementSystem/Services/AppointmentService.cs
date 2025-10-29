using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;


namespace HospitalManagementSystem.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly DapperDbConnectionFactory _connectionFactory;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(DapperDbConnectionFactory connectionFactory, ILogger<AppointmentService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        a.Id,
                        a.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        p.DocumentNumber AS PatientDocument,
                        a.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        a.AppointmentDate,
                        a.Status,
                        a.Reason,
                        a.CreatedAt
                    FROM Appointments a
                    INNER JOIN Patients p ON a.PatientId = p.Id
                    INNER JOIN Doctors d ON a.DoctorId = d.Id
                    WHERE a.Status != 'Cancelled'
                    ORDER BY a.AppointmentDate DESC";

                var appointments = await connection.QueryAsync<AppointmentDto>(sql);

                _logger.LogInformation("Obtenidas {Count} citas", appointments.Count());
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las citas");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(Guid patientId)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        a.Id,
                        a.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        p.DocumentNumber AS PatientDocument,
                        a.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        a.AppointmentDate,
                        a.Status,
                        a.Reason,
                        a.CreatedAt
                    FROM Appointments a
                    INNER JOIN Patients p ON a.PatientId = p.Id
                    INNER JOIN Doctors d ON a.DoctorId = d.Id
                    WHERE a.PatientId = @PatientId
                    ORDER BY a.AppointmentDate DESC";

                var appointments = await connection.QueryAsync<AppointmentDto>(sql, new { PatientId = patientId });

                _logger.LogInformation("Obtenidas {Count} citas para el paciente {PatientId}", appointments.Count(), patientId);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del paciente {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(Guid doctorId)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var appointments = await connection.QueryAsync<AppointmentDto>(
                    "sp_GetAppointmentsByDoctor",
                    new { DoctorId = doctorId },
                    commandType: System.Data.CommandType.StoredProcedure);

                _logger.LogInformation("Obtenidas {Count} citas para el doctor {DoctorId}", appointments.Count(), doctorId);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del doctor {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(Guid id)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        a.Id,
                        a.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        p.DocumentNumber AS PatientDocument,
                        a.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        a.AppointmentDate,
                        a.Status,
                        a.Reason,
                        a.CreatedAt
                    FROM Appointments a
                    INNER JOIN Patients p ON a.PatientId = p.Id
                    INNER JOIN Doctors d ON a.DoctorId = d.Id
                    WHERE a.Id = @Id";

                var appointment = await connection.QueryFirstOrDefaultAsync<AppointmentDto>(sql, new { Id = id });

                if (appointment != null)
                {
                    _logger.LogInformation("Cita obtenida: {AppointmentId}", id);
                }
                else
                {
                    _logger.LogWarning("Cita no encontrada: {AppointmentId}", id);
                }

                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cita por ID: {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    PatientId = createAppointmentDto.PatientId,
                    DoctorId = createAppointmentDto.DoctorId,
                    AppointmentDate = createAppointmentDto.AppointmentDate,
                    Reason = createAppointmentDto.Reason,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var sql = @"
                    INSERT INTO Appointments (Id, PatientId, DoctorId, AppointmentDate, Status, Reason, CreatedAt, UpdatedAt)
                    VALUES (@Id, @PatientId, @DoctorId, @AppointmentDate, @Status, @Reason, @CreatedAt, @UpdatedAt)";

                await connection.ExecuteAsync(sql, appointment);

                _logger.LogInformation("Cita creada exitosamente: {AppointmentId}", appointment.Id);

                return await GetAppointmentByIdAsync(appointment.Id)
                    ?? throw new Exception("Error al recuperar la cita creada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cita");
                throw;
            }
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid id, string status)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    throw new ArgumentException("Estado de cita no válido");
                }

                var sql = @"
                    UPDATE Appointments 
                    SET Status = @Status, UpdatedAt = GETDATE()
                    WHERE Id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, Status = status });

                var success = rowsAffected > 0;

                if (success)
                {
                    _logger.LogInformation("Estado de cita actualizado: {AppointmentId} -> {Status}", id, status);
                }
                else
                {
                    _logger.LogWarning("Cita no encontrada para actualizar estado: {AppointmentId}", id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de cita: {AppointmentId}", id);
                throw;
            }
        }

        public async Task<bool> CancelAppointmentAsync(Guid id)
        {
            return await UpdateAppointmentStatusAsync(id, "Cancelled");
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByStatusAsync(string status)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        a.Id,
                        a.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        p.DocumentNumber AS PatientDocument,
                        a.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        a.AppointmentDate,
                        a.Status,
                        a.Reason,
                        a.CreatedAt
                    FROM Appointments a
                    INNER JOIN Patients p ON a.PatientId = p.Id
                    INNER JOIN Doctors d ON a.DoctorId = d.Id
                    WHERE a.Status = @Status
                    ORDER BY a.AppointmentDate DESC";

                var appointments = await connection.QueryAsync<AppointmentDto>(sql, new { Status = status });

                _logger.LogInformation("Obtenidas {Count} citas con estado: {Status}", appointments.Count(), status);
                return appointments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas por estado: {Status}", status);
                throw;
            }
        }
    }
}
