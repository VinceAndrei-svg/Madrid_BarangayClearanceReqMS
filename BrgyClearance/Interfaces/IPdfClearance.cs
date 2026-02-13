using Proj1.DTOs;

namespace Proj1.Interfaces;

public interface IPdfClearanceService
{
    /// <summary>
    /// Generates a PDF clearance document for a request
    /// </summary>
    Task<ServiceResult<string>> GenerateClearancePdfAsync(int requestId);
    
    /// <summary>
    /// Regenerates a PDF clearance document (if already exists)
    /// </summary>
    Task<ServiceResult<string>> RegenerateClearancePdfAsync(int requestId);
}