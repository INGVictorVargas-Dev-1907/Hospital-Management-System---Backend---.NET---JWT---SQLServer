using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models;

public partial class MedicalRecord
{
    public Guid Id { get; set; }

    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public string Diagnosis { get; set; } = null!;

    public string? Treatment { get; set; }

    public string? Prescription { get; set; }

    public string? Notes { get; set; }

    public DateTime? RecordDate { get; set; } = DateTime.UtcNow;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; } = null!;

    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; } = null!;

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; } = null!;
}
