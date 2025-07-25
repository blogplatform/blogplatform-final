using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogPlatform.Core.Models;

public class ImageUploadResponse
{
    public bool Success { get; set; }
    
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
    
    public string? Message { get; set; }
}

public class S3ImageInfo
{
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public string LastModified { get; set; } = string.Empty;
}

public class S3ImagesListResponse
{
    public bool Success { get; set; }
    public List<S3ImageInfo> Images { get; set; } = new();
}