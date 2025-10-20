namespace AlphaCredit.Api.Models;

public class FileStorageSettings
{
    public string DniStoragePath { get; set; } = "$env:DNI_STORAGE_PATH";
    public string AllowedDniFileTypes { get; set; } = ".jpg,.jpeg,.png,.pdf";
    public int MaxFileSizeInMB { get; set; } = 5;

    public string[] GetAllowedExtensions()
    {
        return AllowedDniFileTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToArray();
    }

    public long GetMaxFileSizeInBytes()
    {
        return MaxFileSizeInMB * 1024 * 1024;
    }
}
