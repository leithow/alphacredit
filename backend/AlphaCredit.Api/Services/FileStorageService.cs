using Microsoft.Extensions.Options;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveDniFileAsync(IFormFile file, long personaId, string tipoDocumento);
    Task<bool> DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
    string GetFullPath(string relativePath);
}

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<FileStorageSettings> settings, ILogger<FileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Crear el directorio si no existe
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        var fullPath = Path.GetFullPath(_settings.DniStoragePath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            _logger.LogInformation("Directorio de almacenamiento DNI creado: {Path}", fullPath);
        }
    }

    public async Task<string> SaveDniFileAsync(IFormFile file, long personaId, string tipoDocumento)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("El archivo está vacío o no existe");
        }

        // Validar extensión
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _settings.GetAllowedExtensions();

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Tipo de archivo no permitido. Extensiones permitidas: {string.Join(", ", allowedExtensions)}"
            );
        }

        // Validar tamaño
        if (file.Length > _settings.GetMaxFileSizeInBytes())
        {
            throw new InvalidOperationException(
                $"El archivo excede el tamaño máximo permitido de {_settings.MaxFileSizeInMB}MB"
            );
        }

        // Generar nombre único del archivo
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomString = Guid.NewGuid().ToString("N").Substring(0, 8);
        var safeFileName = $"persona_{personaId}_{tipoDocumento}_{timestamp}_{randomString}{extension}";

        // Ruta completa del archivo
        var fullPath = Path.GetFullPath(_settings.DniStoragePath);
        var filePath = Path.Combine(fullPath, safeFileName);

        try
        {
            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retornar ruta relativa para almacenar en la base de datos
            var relativePath = Path.Combine(_settings.DniStoragePath, safeFileName);
            _logger.LogInformation("Archivo DNI guardado: {Path}", relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo DNI");
            throw new InvalidOperationException("Error al guardar el archivo", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Archivo eliminado: {Path}", filePath);
                return true;
            }

            _logger.LogWarning("Archivo no encontrado para eliminar: {Path}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo: {Path}", filePath);
            return false;
        }
    }

    public bool FileExists(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        return File.Exists(fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        return Path.GetFullPath(relativePath);
    }
}
