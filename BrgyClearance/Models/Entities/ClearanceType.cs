using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Proj1.Models.Common;

namespace Proj1.Models.Entities;

/// <summary>
/// Represents a type of clearance that can be requested
/// </summary>
public class ClearanceType : BaseEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// Display name of the clearance type
    /// Uses [Column("TypeName")] to map to existing database column without migration
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("TypeName")]  // ✅ Maps to existing DB column - NO MIGRATION NEEDED
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; }
    
    /// <summary>
    /// Expected processing time in days
    /// </summary>
    [Required]
    [Range(1, 365)]
    public int ProcessingDays { get; set; } = 3;
    
    /// <summary>
    /// Whether this clearance type is currently offered
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ClearanceRequest> ClearanceRequests { get; set; } = new List<ClearanceRequest>();
}