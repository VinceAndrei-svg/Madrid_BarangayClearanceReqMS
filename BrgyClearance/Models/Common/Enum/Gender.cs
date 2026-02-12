using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.Common.Enums;

/// <summary>
/// Represents gender options for residents.
/// Using Display attribute for user-friendly names in UI.
/// </summary>
public enum Gender
{
    [Display(Name = "Male")]
    Male = 0,
    
    [Display(Name = "Female")]
    Female = 1,
    
    [Display(Name = "Other")]
    Other = 2
}
