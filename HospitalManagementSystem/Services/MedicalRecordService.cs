using HospitalManagementSystem.Data;
using HospitalManagementSystem.DTOs;
using QuestPDF.Infrastructure;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Dapper;

namespace HospitalManagementSystem.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly DapperDbConnectionFactory _connectionFactory;
        private readonly ILogger<MedicalRecordService> _logger;

        // Actualizar el constructor
        public MedicalRecordService(DapperDbConnectionFactory connectionFactory, ILogger<MedicalRecordService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

            // Configurar QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetPatientMedicalHistoryAsync(Guid patientId)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var medicalRecords = await connection.QueryAsync<MedicalRecordDto>(
                    "sp_GetPatientMedicalHistory",
                    new { PatientId = patientId },
                    commandType: CommandType.StoredProcedure);

                _logger.LogInformation("Obtenido historial médico para paciente {PatientId} - {Count} registros",
                    patientId, medicalRecords.Count());
                return medicalRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial médico del paciente {PatientId}", patientId);
                throw;
            }
        }

        public async Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(Guid id)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        mr.Id,
                        mr.AppointmentId,
                        a.AppointmentDate,
                        mr.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        mr.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        mr.Diagnosis,
                        mr.Treatment,
                        mr.Prescription,
                        mr.Notes,
                        mr.RecordDate
                    FROM MedicalRecords mr
                    INNER JOIN Appointments a ON mr.AppointmentId = a.Id
                    INNER JOIN Patients p ON mr.PatientId = p.Id
                    INNER JOIN Doctors d ON mr.DoctorId = d.Id
                    WHERE mr.Id = @Id";

                var medicalRecord = await connection.QueryFirstOrDefaultAsync<MedicalRecordDto>(sql, new { Id = id });

                if (medicalRecord != null)
                {
                    _logger.LogInformation("Registro médico obtenido: {MedicalRecordId}", id);
                }
                else
                {
                    _logger.LogWarning("Registro médico no encontrado: {MedicalRecordId}", id);
                }

                return medicalRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registro médico por ID: {MedicalRecordId}", id);
                throw;
            }
        }

        public async Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordDto createMedicalRecordDto)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                // Primero obtener información de la cita
                var appointmentSql = @"
                    SELECT PatientId, DoctorId 
                    FROM Appointments 
                    WHERE Id = @AppointmentId AND Status = 'Scheduled'";

                var appointment = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    appointmentSql, new { createMedicalRecordDto.AppointmentId });

                if (appointment == null)
                {
                    throw new ArgumentException("Cita no encontrada o ya completada");
                }

                // Crear el registro médico usando el stored procedure
                var parameters = new
                {
                    createMedicalRecordDto.AppointmentId,
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    createMedicalRecordDto.Diagnosis,
                    createMedicalRecordDto.Treatment,
                    createMedicalRecordDto.Prescription,
                    createMedicalRecordDto.Notes
                };

                var medicalRecord = await connection.QueryFirstOrDefaultAsync<MedicalRecordDto>(
                    "sp_CreateMedicalRecord",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (medicalRecord == null)
                {
                    throw new Exception("Error al crear el registro médico");
                }

                _logger.LogInformation("Registro médico creado exitosamente: {MedicalRecordId}", medicalRecord.Id);

                return await GetMedicalRecordByIdAsync(medicalRecord.Id)
                    ?? throw new Exception("Error al recuperar el registro médico creado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro médico para la cita {AppointmentId}",
                    createMedicalRecordDto.AppointmentId);
                throw;
            }
        }

        public async Task<byte[]> GenerateMedicalHistoryPdfAsync(Guid patientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Obtener información del paciente
                var patient = await GetPatientInfoAsync(patientId);
                if (patient == null)
                {
                    throw new ArgumentException("Paciente no encontrado");
                }

                // Obtener historial médico
                var medicalHistory = await GetPatientMedicalHistoryAsync(patientId);

                // Filtrar por fecha si se especifica
                if (startDate.HasValue)
                {
                    medicalHistory = medicalHistory.Where(m => m.RecordDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    medicalHistory = medicalHistory.Where(m => m.RecordDate <= endDate.Value);
                }

                // Generar PDF
                var pdfDocument = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .AlignCenter()
                            .Text("HISTORIAL MÉDICO")
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Item().Text($"Paciente: {patient.FirstName} {patient.LastName}");
                                column.Item().Text($"Documento: {patient.DocumentNumber}");
                                column.Item().Text($"Fecha de Nacimiento: {patient.BirthDate:dd/MM/yyyy}");
                                column.Item().Text($"Género: {patient.Gender}");
                                column.Item().Text($"Email: {patient.Email}");
                                column.Item().Text($"Teléfono: {patient.PhoneNumber ?? "No especificado"}");

                                column.Item().PaddingTop(10).Text("HISTORIAL DE CONSULTAS").SemiBold();

                                if (!medicalHistory.Any())
                                {
                                    column.Item().PaddingTop(5).Text("No hay registros médicos disponibles.");
                                }
                                else
                                {
                                    foreach (var record in medicalHistory)
                                    {
                                        column.Item().PaddingTop(10).Background(Colors.Grey.Lighten3).Padding(10).Column(recordColumn =>
                                        {
                                            recordColumn.Item().Text($"Fecha: {record.RecordDate:dd/MM/yyyy}").SemiBold();
                                            recordColumn.Item().Text($"Doctor: {record.DoctorName} - {record.DoctorSpecialty}");
                                            recordColumn.Item().Text($"Diagnóstico: {record.Diagnosis}");

                                            if (!string.IsNullOrEmpty(record.Treatment))
                                            {
                                                recordColumn.Item().Text($"Tratamiento: {record.Treatment}");
                                            }

                                            if (!string.IsNullOrEmpty(record.Prescription))
                                            {
                                                recordColumn.Item().Text($"Prescripción: {record.Prescription}");
                                            }

                                            if (!string.IsNullOrEmpty(record.Notes))
                                            {
                                                recordColumn.Item().Text($"Notas: {record.Notes}");
                                            }
                                        });
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generado el: ");
                                x.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                                x.Span(" - Página ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                return pdfDocument.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF del historial médico para paciente {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<MedicalRecordDto>> GetAllMedicalRecordsAsync()
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        mr.Id,
                        mr.AppointmentId,
                        a.AppointmentDate,
                        mr.PatientId,
                        p.FirstName + ' ' + p.LastName AS PatientName,
                        mr.DoctorId,
                        d.FirstName + ' ' + d.LastName AS DoctorName,
                        d.Specialty AS DoctorSpecialty,
                        mr.Diagnosis,
                        mr.Treatment,
                        mr.Prescription,
                        mr.Notes,
                        mr.RecordDate
                    FROM MedicalRecords mr
                    INNER JOIN Appointments a ON mr.AppointmentId = a.Id
                    INNER JOIN Patients p ON mr.PatientId = p.Id
                    INNER JOIN Doctors d ON mr.DoctorId = d.Id
                    ORDER BY mr.RecordDate DESC";

                var medicalRecords = await connection.QueryAsync<MedicalRecordDto>(sql);

                _logger.LogInformation("Obtenidos {Count} registros médicos", medicalRecords.Count());
                return medicalRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los registros médicos");
                throw;
            }
        }

        private async Task<dynamic?> GetPatientInfoAsync(Guid patientId)
        {
            // Usar _connectionFactory
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT 
                    p.FirstName, p.LastName, p.DocumentNumber, 
                    p.BirthDate, p.Gender, p.Email, p.PhoneNumber
                FROM Patients p 
                WHERE p.Id = @PatientId AND p.IsActive = 1";

            return await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { PatientId = patientId });
        }
    }
}
