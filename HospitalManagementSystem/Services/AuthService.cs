using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.DTOs;
using HospitalManagementSystem.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using Microsoft.Extensions.Logging; // Necesario para ILogger
using System;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        // CAMBIO: Ahora inyectamos la fábrica de conexiones Dapper.
        private readonly DapperDbConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // CAMBIO: El constructor recibe la nueva fábrica.
        public AuthService(DapperDbConnectionFactory connectionFactory, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Usamos la fábrica para obtener la conexión.
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                SELECT 
                    u.Id, u.UserName, u.Email, u.PasswordHash, u.IsActive,
                    r.Name as RoleName, r.Id as RoleId
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.Id 
                WHERE u.Email = @Email AND u.IsActive = 1";

            var user = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { loginDto.Email });

            if (user == null)
            {
                _logger.LogWarning("Intento de login fallido: Usuario no encontrado - {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Encriptar contraseña
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Intento de login fallido: Contraseña incorrecta - {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Intento de login fallido: Usuario inactivo - {Email}", loginDto.Email);
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            var token = GenerateJwtToken(user);

            _logger.LogInformation("Login exitoso: {Email} - Rol: {Role}", (string)user.Email, (string)user.RoleName);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.RoleName
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await UserExistsAsync(registerDto.Email))
            {
                throw new ArgumentException("El usuario con este email ya existe");
            }

            // Usamos la fábrica para obtener la conexión.
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Obtener RoleId
                var roleSql = "SELECT Id FROM Roles WHERE Name = @RoleName";
                var roleId = await connection.QueryFirstOrDefaultAsync<int?>(roleSql, new { RoleName = registerDto.Role }, transaction);

                if (roleId == null)
                {
                    throw new ArgumentException("Rol inválido");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    RoleId = roleId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var insertUserSql = @"
                    INSERT INTO Users (Id, UserName, Email, PasswordHash, RoleId, CreatedAt, UpdatedAt)
                    VALUES (@Id, @UserName, @Email, @PasswordHash, @RoleId, @CreatedAt, @UpdatedAt)";

                await connection.ExecuteAsync(insertUserSql, user, transaction);

                // Crear registro específico según el rol
                if (registerDto.Role == "Patient")
                {
                    await CreatePatientRecord(connection, transaction, registerDto, user.Id);
                }
                else if (registerDto.Role == "Doctor")
                {
                    await CreateDoctorRecord(connection, transaction, registerDto, user.Id);
                }

                transaction.Commit();

                _logger.LogInformation("Registro exitoso: {Email} - Rol: {Role}", user.Email, registerDto.Role);

                // Auto-login después del registro
                var loginDto = new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };

                return await LoginAsync(loginDto);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error durante el registro para {Email}", registerDto.Email);
                throw;
            }
        }

        private async Task CreatePatientRecord(IDbConnection connection, IDbTransaction transaction, RegisterDto registerDto, Guid userId)
        {
            if (registerDto.DocumentTypeId == null || string.IsNullOrEmpty(registerDto.DocumentNumber) ||
                string.IsNullOrEmpty(registerDto.FirstName) || string.IsNullOrEmpty(registerDto.LastName) ||
                registerDto.BirthDate == null || string.IsNullOrEmpty(registerDto.Gender))
            {
                throw new ArgumentException("Todos los campos del paciente son requeridos");
            }

            var patientSql = @"
                INSERT INTO Patients (Id, DocumentTypeId, DocumentNumber, FirstName, LastName, 
                                     BirthDate, Email, Gender, Address, PhoneNumber, UserId)
                VALUES (NEWID(), @DocumentTypeId, @DocumentNumber, @FirstName, @LastName, 
                        @BirthDate, @Email, @Gender, @Address, @PhoneNumber, @UserId)";

            await connection.ExecuteAsync(patientSql, new
            {
                DocumentTypeId = registerDto.DocumentTypeId,
                DocumentNumber = registerDto.DocumentNumber,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                BirthDate = registerDto.BirthDate,
                Email = registerDto.Email,
                Gender = registerDto.Gender,
                Address = registerDto.Address,
                PhoneNumber = registerDto.PhoneNumber,
                UserId = userId
            }, transaction);
        }

     
        private async Task CreateDoctorRecord(IDbConnection connection, IDbTransaction transaction, RegisterDto registerDto, Guid userId)
        {
            if (string.IsNullOrEmpty(registerDto.Specialty) || string.IsNullOrEmpty(registerDto.LicenseNumber))
            {
                throw new ArgumentException("Especialidad y número de licencia son requeridos para doctores");
            }

            var doctorSql = @"
                INSERT INTO Doctors (Id, DocumentTypeId, DocumentNumber, FirstName, LastName, 
                                     Specialty, LicenseNumber, Email, PhoneNumber, UserId)
                VALUES (NEWID(), @DocumentTypeId, @DocumentNumber, @FirstName, @LastName, 
                        @Specialty, @LicenseNumber, @Email, @PhoneNumber, @UserId)";

            await connection.ExecuteAsync(doctorSql, new
            {
                DocumentTypeId = registerDto.DocumentTypeId ?? 1, // Default a CC (Asumiendo que 1 es un tipo de documento válido)
                DocumentNumber = registerDto.DocumentNumber ?? "TEMP",
                FirstName = registerDto.FirstName ?? registerDto.UserName,
                LastName = registerDto.LastName ?? " ",
                Specialty = registerDto.Specialty,
                LicenseNumber = registerDto.LicenseNumber,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserId = userId
            }, transaction);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            // Usamos la fábrica para obtener la conexión.
            using var connection = _connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
            return exists;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            // Usamos la fábrica para obtener la conexión.
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT u.Id, u.UserName, u.Email, r.Name as Role
                FROM Users u 
                INNER JOIN Roles r ON u.RoleId = r.Id 
                WHERE u.Id = @UserId AND u.IsActive = 1";

            var user = await connection.QueryFirstOrDefaultAsync<UserDto>(sql, new { UserId = userId });
            return user;
        }

        private string GenerateJwtToken(dynamic user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Obtener la clave secreta del appsettings.json
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleName)
                }),
                // Usar el valor de ExpireHours del appsettings.json
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}