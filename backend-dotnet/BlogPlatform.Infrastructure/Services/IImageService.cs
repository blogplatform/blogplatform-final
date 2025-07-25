using BlogPlatform.Core.Models;
using Microsoft.AspNetCore.Http;

namespace BlogPlatform.Infrastructure.Services;

public interface IImageService
{
    Task<ImageUploadResponse> UploadImageAsync(IFormFile file);
    Task<S3ImagesListResponse> ListImagesAsync(string prefix = "uploads/");
    Task<string> GetImageUrlAsync(string fileKey);
    string GenerateS3Url(string key);
}