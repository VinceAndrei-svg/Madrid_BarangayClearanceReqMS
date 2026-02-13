using System.ComponentModel.DataAnnotations;

namespace Proj1.Models.Common.Enums;

/// <summary>
/// Philippine civil status categories as per PSA standards
/// </summary>
public enum CivilStatus
{
    [Display(Name = "Single")]
    Single = 1,
    
    [Display(Name = "Married")]
    Married = 2,
    
    [Display(Name = "Widowed")]
    Widowed = 3,
    
    [Display(Name = "Separated")]
    Separated = 4,
    
    [Display(Name = "Divorced")]
    Divorced = 5,
    
    [Display(Name = "Annulled")]
    Annulled = 6,
    
    [Display(Name = "Live-in")]
    LiveIn = 7
}