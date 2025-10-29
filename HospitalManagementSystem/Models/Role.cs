using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models;

public partial class Role
{
    
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(255)]
    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
