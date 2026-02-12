using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.Common.Enums;

/// <summary>
/// Represents civil status options for residents.
/// </summary>
public enum CivilStatus
{
    [Display(Name = "Single")]
    Single = 0,
    
    [Display(Name = "Married")]
    Married = 1,
    
    [Display(Name = "Widowed")]
    Widowed = 2,
    
    [Display(Name = "Divorced")]
    Divorced = 3,
    
    [Display(Name = "Separated")]
    Separated = 4
}
