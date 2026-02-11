using Proj1.Models.Common;

namespace Proj1.Models.Entities;

public class Resident : BaseEntity
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}