namespace Proj1.Models.Configuration;

public class ClearanceSettings
{
    public int ValidityMonths { get; set; } = 6;
    public decimal DocumentaryStampTax { get; set; } = 30.00m;
}