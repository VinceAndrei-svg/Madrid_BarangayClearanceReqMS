namespace Proj1.Helpers;

/// <summary>
/// Helper class for generating unique reference numbers for clearance requests.
/// Best Practice: Extract complex logic into helper classes for reusability and testability.
/// </summary>
public static class ReferenceNumberGenerator
{
    /// <summary>
    /// Generates a unique reference number in the format: BRG-YYYY-NNNNN
    /// Example: BRG-2024-00001, BRG-2024-00002
    /// </summary>
    /// <param name="sequenceNumber">The sequence number for this year (1, 2, 3, etc.)</param>
    /// <returns>Formatted reference number</returns>
    public static string Generate(int sequenceNumber)
    {
        var year = DateTime.UtcNow.Year;
        var paddedNumber = sequenceNumber.ToString("D5"); // Pads with zeros: 1 -> 00001
        
        return $"BRG-{year}-{paddedNumber}";
    }
    
    /// <summary>
    /// Extracts the year from a reference number.
    /// Example: "BRG-2024-00001" returns 2024
    /// </summary>
    public static int? ExtractYear(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            return null;
            
        var parts = referenceNumber.Split('-');
        if (parts.Length != 3)
            return null;
            
        if (int.TryParse(parts[1], out var year))
            return year;
            
        return null;
    }
    
    /// <summary>
    /// Extracts the sequence number from a reference number.
    /// Example: "BRG-2024-00001" returns 1
    /// </summary>
    public static int? ExtractSequence(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            return null;
            
        var parts = referenceNumber.Split('-');
        if (parts.Length != 3)
            return null;
            
        if (int.TryParse(parts[2], out var sequence))
            return sequence;
            
        return null;
    }
}
