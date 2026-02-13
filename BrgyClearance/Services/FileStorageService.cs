using Microsoft.Extensions.Options;
using Proj1.DTOs;
using Proj1.Interfaces;
using Proj1.Models.Configuration;

namespace Proj1.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ServiceResult<string>> SaveFileAsync(byte[] fileContent, string fileName, string folder)
    {
        try
        {
            // Validate file size
            var fileSizeInMB = fileContent.Length / (1024.0 * 1024.0);
            if (fileSizeInMB > _settings.MaxFileSizeInMB)
            {
                return ServiceResult<string>.Failure($"File size exceeds maximum allowed size of {_settings.MaxFileSizeInMB}MB");
            }

            // Sanitize filename
            var sanitizedFileName = Path.GetFileName(fileName);
            
            // Create directory if it doesn't exist
            var folderPath = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Created directory: {FolderPath}", folderPath);
            }

            // Full file path
            var filePath = Path.Combine(folderPath, sanitizedFileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, fileContent);
            
            _logger.LogInformation("File saved successfully: {FileName}", sanitizedFileName);

            // Return relative web path
            var webPath = $"/{folder}/{sanitizedFileName}";
            return ServiceResult<string>.Success(webPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            return ServiceResult<string>.Failure($"Error saving file: {ex.Message}");
        }
    }

    public string GetPhysicalPath(string relativePath)
    {
        var cleanPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_environment.WebRootPath, cleanPath);
    }

    public string GetWebPath(string fileName, string folder)
    {
        return $"/{folder}/{fileName}";
    }

    public async Task<ServiceResult> DeleteFileAsync(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);
            
            if (File.Exists(physicalPath))
            {
                await Task.Run(() => File.Delete(physicalPath));
                _logger.LogInformation("File deleted: {Path}", relativePath);
                return ServiceResult.Success("File deleted successfully");
            }

            return ServiceResult.Failure("File not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Path}", relativePath);
            return ServiceResult.Failure($"Error deleting file: {ex.Message}");
        }
    }

    public bool FileExists(string relativePath)
    {
        var physicalPath = GetPhysicalPath(relativePath);
        return File.Exists(physicalPath);
    }
}