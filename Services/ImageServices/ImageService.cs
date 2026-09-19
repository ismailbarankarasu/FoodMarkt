using MongoDB.Bson;
using MongoDB.Driver;

namespace FoodMart.Services.ImageServices;

public sealed class ImageService(IWebHostEnvironment environment, IMongoDatabase database, ILogger<ImageService> logger)
{
    public const long MaxBytes = 5 * 1024 * 1024;

    public async Task<string?> ValidateAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxBytes) return "Görsel boyutu 1 bayt ile 5 MB arasında olmalıdır.";
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var expected = extension switch { ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", _ => null };
        if (expected is null || file.ContentType != expected) return "Yalnızca JPG, PNG ve WebP görselleri yüklenebilir.";
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        var valid = read == 12 && (expected switch
        {
            "image/jpeg" => header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            "image/png" => header.AsSpan(0, 8).SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10}),
            "image/webp" => header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        });
        return valid ? null : "Dosya içeriği seçilen görsel türüyle eşleşmiyor.";
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(file, cancellationToken);
        if (error is not null) throw new InvalidOperationException(error);
        var directory = Path.Combine(environment.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(directory);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        var path = Path.Combine(directory, name);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
        return "/uploads/images/" + name;
    }

    // URLs can be reused by several records; remove only our generated, unreferenced files.
    public async Task DeleteIfUnusedAsync(string? url)
    {
        if (url is null || !url.StartsWith("/uploads/images/", StringComparison.Ordinal)) return;
        var name = url["/uploads/images/".Length..];
        if (Path.GetFileName(name) != name || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _)) return;
        try
        {
            foreach (var collection in new[] { "Products", "Features", "Discounts" })
                if (await database.GetCollection<BsonDocument>(collection).Find(new BsonDocument("ImageUrl", url)).AnyAsync()) return;
            File.Delete(Path.Combine(environment.WebRootPath, "uploads", "images", name));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or MongoException)
        {
            logger.LogWarning("Unused image cleanup failed ({ErrorType}).", ex.GetType().Name);
        }
    }
}
