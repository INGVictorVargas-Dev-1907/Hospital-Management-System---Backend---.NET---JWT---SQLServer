using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTOs
{
    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = "El ID del paciente es requerido")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "El ID del doctor es requerido")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string? Reason { get; set; }
    }

    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientDocument { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMedicalRecordDto
    {
        [Required(ErrorMessage = "El ID de la cita es requerido")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "El diagnóstico es requerido")]
        public string Diagnosis { get; set; } = string.Empty;

        public string? Treatment { get; set; }
        public string? Prescription { get; set; }
        public string? Notes { get; set; }
    }

    public class MedicalRecordDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialty { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        public string? Prescription { get; set; }
        public string? Notes { get; set; }
        public DateTime RecordDate { get; set; }
    }

    public class MedicalHistoryPdfRequestDto
    {
        [Required(ErrorMessage = "El ID del paciente es requerido")]
        public Guid PatientId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
