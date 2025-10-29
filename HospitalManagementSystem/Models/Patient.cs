using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models;

public partial class Patient
{
    public Guid Id { get; set; }

    [Required]
    public int DocumentTypeId { get; set; }

    [Required]
    [MaxLength(20)]
    public string DocumentNumber { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = null!;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ForeignKey("DocumentTypeId")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
