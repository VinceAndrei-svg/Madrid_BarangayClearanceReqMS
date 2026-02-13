using Proj1.DTOs;

namespace Proj1.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the storage location
    /// </summary>
    Task<ServiceResult<string>> SaveFileAsync(byte[] fileContent, string fileName, string folder);
    
    /// <summary>
    /// Gets the physical path for a stored file
    /// </summary>
    string GetPhysicalPath(string relativePath);
    
    /// <summary>
    /// Gets the web path for a stored file
    /// </summary>
    string GetWebPath(string fileName, string folder);
    
    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task<ServiceResult> DeleteFileAsync(string relativePath);
    
    /// <summary>
    /// Checks if a file exists
    /// </summary>
    bool FileExists(string relativePath);
}