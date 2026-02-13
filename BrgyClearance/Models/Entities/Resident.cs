using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Proj1.Models.Common;

namespace Proj1.Models.Entities;

/// <summary>
/// Represents a resident registered in the barangay system
/// </summary>
public class Resident : BaseEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to AspNetUsers
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full address within the barangay
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    public DateTime BirthDate { get; set; }
    
    // Navigation properties
    public ICollection<ClearanceRequest> ClearanceRequests { get; set; } = new List<ClearanceRequest>();
    
    /// <summary>
    /// Gets the full name
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}