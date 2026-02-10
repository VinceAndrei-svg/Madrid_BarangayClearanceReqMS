using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.ViewModels;

public class EditResidentViewModel
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
}