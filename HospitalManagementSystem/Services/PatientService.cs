using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;
using System.Data;
using System.Linq;

namespace HospitalManagementSystem.Services
{
    public class PatientService : IPatientService
    {
        private readonly DapperDbConnectionFactory _connectionFactory;
        private readonly ILogger<PatientService> _logger;

        public PatientService(DapperDbConnectionFactory connectionFactory, ILogger<PatientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var patients = await connection.QueryAsync<PatientDto>(
                        "sp_GetAllPatients",
                        commandType: CommandType.StoredProcedure);
                
                _logger.LogInformation("Obtenidos {Count} pacientes", patients.Count());
                return patients;
            
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los pacientes");
                throw;
            }
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var parameters = new { PatientId = id };
                var patient = await connection.QueryFirstOrDefaultAsync<PatientDto>(
                    "sp_GetPatientById",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (patient != null)
                {
                    _logger.LogInformation("Paciente obtenido: {PatientId}", id);
                }
                else
                {
                    _logger.LogWarning("Paciente no encontrado: {PatientId}", id);
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente por ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto)
        {
            
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Primero crear el usuario
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    // NOTA: BCrypt y la clase User deben estar disponibles.
                    UserName = createPatientDto.Email.Split('@')[0],
                    Email = createPatientDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TempPassword123!"), // Password temporal
                    RoleId = 3, // Patient role
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var insertUserSql = @"
                    INSERT INTO Users (Id, UserName, Email, PasswordHash, RoleId, CreatedAt, UpdatedAt)
                    VALUES (@Id, @UserName, @Email, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt)";

                await connection.ExecuteAsync(insertUserSql, user, transaction);

                // Luego crear el paciente usando el stored procedure
                var parameters = new
                {
                    createPatientDto.DocumentTypeId,
                    createPatientDto.DocumentNumber,
                    createPatientDto.FirstName,
                    createPatientDto.LastName,
                    BirthDate = createPatientDto.BirthDate.Date,
                    createPatientDto.Email,
                    createPatientDto.Gender,
                    createPatientDto.Address,
                    createPatientDto.PhoneNumber,
                    UserId = user.Id
                };

                var patient = await connection.QueryFirstOrDefaultAsync<PatientDto>(
                    "sp_CreatePatient",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);

                if (patient == null)
                {
                    throw new Exception("Error al crear el paciente");
                }

                transaction.Commit();

                _logger.LogInformation("Paciente creado exitosamente: {DocumentNumber}", createPatientDto.DocumentNumber);

                return await GetPatientByIdAsync(patient.Id);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error al crear paciente con documento: {DocumentNumber}", createPatientDto.DocumentNumber);
                throw;
            }
        }

        public async Task<PatientDto?> UpdatePatientAsync(Guid id, CreatePatientDto updatePatientDto)
        {
            
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var parameters = new
                {
                    PatientId = id,
                    updatePatientDto.DocumentTypeId,
                    updatePatientDto.DocumentNumber,
                    updatePatientDto.FirstName,
                    updatePatientDto.LastName,
                    BirthDate = updatePatientDto.BirthDate.Date,
                    updatePatientDto.Email,
                    updatePatientDto.Gender,
                    updatePatientDto.Address,
                    updatePatientDto.PhoneNumber
                };

                var patient = await connection.QueryFirstOrDefaultAsync<PatientDto>(
                    "sp_UpdatePatient",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // ... (lógica de logging) ...
                if (patient != null)
                {
                    _logger.LogInformation("Paciente actualizado: {PatientId}", id);
                }
                else
                {
                    _logger.LogWarning("Paciente no encontrado para actualizar: {PatientId}", id);
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar paciente: {PatientId}", id);
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(Guid id)
        {
          
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var parameters = new { PatientId = id };
                var rowsAffected = await connection.ExecuteScalarAsync<int>(
                    "sp_DeletePatient",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var success = rowsAffected > 0;

                // ... (lógica de logging) ...
                if (success)
                {
                    _logger.LogInformation("Paciente eliminado: {PatientId}", id);
                }
                else
                {
                    _logger.LogWarning("Paciente no encontrado para eliminar: {PatientId}", id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paciente: {PatientId}", id);
                throw;
            }
        }

        public async Task<Guid> GetPatientUserIdAsync(Guid patientId)
        {
            
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = "SELECT UserId FROM Patients WHERE Id = @PatientId";
                var userId = await connection.ExecuteScalarAsync<Guid>(sql, new { PatientId = patientId });
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener UserId del paciente: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm)
        {
          
            using var connection = _connectionFactory.CreateConnection();

            try
            {
                var sql = @"
                    SELECT 
                        p.Id,
                        dt.Name AS DocumentType,
                        p.DocumentNumber,
                        p.FirstName,
                        p.LastName,
                        p.BirthDate,
                        p.Email,
                        p.Gender,
                        p.Address,
                        p.PhoneNumber,
                        p.IsActive,
                        p.CreatedAt
                    FROM Patients p
                    INNER JOIN DocumentTypes dt ON p.DocumentTypeId = dt.Id
                    WHERE p.IsActive = 1 
                    AND (p.FirstName LIKE @SearchTerm 
                         OR p.LastName LIKE @SearchTerm 
                         OR p.DocumentNumber LIKE @SearchTerm
                         OR p.Email LIKE @SearchTerm)
                    ORDER BY p.FirstName, p.LastName";

                var searchPattern = $"%{searchTerm}%";
                var patients = await connection.QueryAsync<PatientDto>(sql, new { SearchTerm = searchPattern });

                _logger.LogInformation("Búsqueda de pacientes: '{SearchTerm}' - Encontrados: {Count}", searchTerm, patients.Count());
                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar pacientes con término: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}
