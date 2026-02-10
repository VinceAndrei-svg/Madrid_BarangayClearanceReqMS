using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.Entities;

public class Resident
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public DateTime BirthDate { get; set; }
}