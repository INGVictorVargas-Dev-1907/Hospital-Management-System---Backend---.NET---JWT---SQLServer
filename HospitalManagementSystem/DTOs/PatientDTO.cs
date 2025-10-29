using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "El tipo de documento es requerido")]
        public int DocumentTypeId { get; set; }

        [Required(ErrorMessage = "El número de documento es requerido")]
        [MaxLength(20, ErrorMessage = "El número de documento no puede exceder 20 caracteres")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El género es requerido")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? Address { get; set; }

        [MaxLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
        public string? PhoneNumber { get; set; }
    }

    public class PatientDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }

    public class UpdatePatientDto : CreatePatientDto
    {
    }
}
