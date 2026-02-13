namespace Proj1.Models.Configuration;

public class FileStorageSettings
{
    public string ClearancesFolder { get; set; } = "clearances";
    public int MaxFileSizeInMB { get; set; } = 10;
    public List<string> AllowedExtensions { get; set; } = new() { ".pdf" };
}