using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    // Define la interfaz para el servicio de autenticación y gestión básica de usuarios.
    public interface IAuthService
    {
        /// <summary>
        /// Intenta autenticar un usuario con las credenciales proporcionadas.
        /// </summary>
        /// <param name="loginDto">Objeto de transferencia de datos con el nombre de usuario/correo y contraseña.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene el token de autenticación (AuthResponseDto) si es exitoso.</returns>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="registerDto">Objeto de transferencia de datos con la información de registro del nuevo usuario.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene la respuesta de autenticación (AuthResponseDto) si el registro es exitoso.</returns>
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Verifica si un usuario con el correo electrónico especificado ya existe en el sistema.
        /// </summary>
        /// <param name="email">El correo electrónico a verificar.</param>
        /// <returns>Tarea que representa la operación asíncrona. Devuelve 'true' si el usuario existe, 'false' en caso contrario.</returns>
        Task<bool> UserExistsAsync(string email);

        /// <summary>
        /// Obtiene los detalles de un usuario por su identificador único (GUID).
        /// </summary>
        /// <param name="userId">El GUID del usuario.</param>
        /// <returns>Tarea que representa la operación asíncrona. Devuelve un UserDto con la información del usuario o 'null' si no se encuentra.</returns>
        Task<UserDto?> GetUserByIdAsync(Guid userId);

    }
}
