using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es requerido")]
        [RegularExpression("^(Patient|Doctor|Employee|Admin)$", ErrorMessage = "Rol no válido")]
        public string Role { get; set; } = string.Empty;

        // Campos específicos para paciente
        public int? DocumentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }

        // Campos específicos para doctor
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
