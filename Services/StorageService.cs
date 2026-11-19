namespace CivicAlert.Api.Services;

public class StorageService(IWebHostEnvironment environment)
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Unsupported image format '{extension}'. Allowed: .jpg, .jpeg, .png, .webp");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new InvalidOperationException("Image file exceeds the 10 MB limit.");
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadDir);

        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(uploadDir, uniqueName);

        await using var stream = File.Create(destination);
        await file.CopyToAsync(stream, ct);

        return $"/uploads/{uniqueName}";
    }
}
