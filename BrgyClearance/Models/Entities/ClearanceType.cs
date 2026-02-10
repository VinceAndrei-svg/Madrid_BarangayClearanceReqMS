using Proj1.Models.Common;

namespace Proj1.Models.Entities;

public class ClearanceType : BaseEntity
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Fee { get; set; }
    public int ProcessingDays { get; set; }
    public bool IsActive { get; set; } = true;
}