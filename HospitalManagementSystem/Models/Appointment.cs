using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models;

public partial class Appointment
{
    public Guid Id { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; } = null!;
}
