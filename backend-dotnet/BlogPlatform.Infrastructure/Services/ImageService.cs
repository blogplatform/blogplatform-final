using Amazon.S3;
using Amazon.S3.Model;
using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BlogPlatform.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public ImageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3BucketName"] ?? throw new InvalidOperationException("S3 bucket name not configured");
        _region = configuration["AWS:Region"] ?? "eu-north-1";
    }

    public async Task<ImageUploadResponse> UploadImageAsync(IFormFile file)
    {
        try
        {
            Console.WriteLine($"Starting image upload. File: {file?.FileName}, Size: {file?.Length}, ContentType: {file?.ContentType}");
            
            // Validate file
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("File validation failed: File is empty");
                return new ImageUploadResponse { Success = false, Message = "File is empty" };
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                Console.WriteLine($"File validation failed: Invalid content type {file.ContentType}");
                return new ImageUploadResponse { Success = false, Message = "Only image files are allowed" };
            }

            if (file.Length > MaxFileSize)
            {
                Console.WriteLine($"File validation failed: File size {file.Length} exceeds limit {MaxFileSize}");
                return new ImageUploadResponse { Success = false, Message = "File exceeds 5MB limit" };
            }

            // Generate unique key
            var fileExtension = Path.GetExtension(file.FileName);
            var key = $"uploads/{Guid.NewGuid():N}{fileExtension}";
            Console.WriteLine($"Generated S3 key: {key}");
            Console.WriteLine($"Using bucket: {_bucketName}, region: {_region}");

            // Upload to S3
            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
                // Note: Removed CannedACL since bucket doesn't allow ACLs
                // The bucket should be configured with public read access via bucket policy
            };

            Console.WriteLine("Attempting S3 upload...");
            await _s3Client.PutObjectAsync(request);
            Console.WriteLine("S3 upload successful!");

            var imageUrl = GenerateS3Url(key);
            Console.WriteLine($"Generated image URL: {imageUrl}");
            
            return new ImageUploadResponse
            {
                Success = true,
                ImageUrl = imageUrl,
                Message = "Image uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Image upload failed with exception: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            return new ImageUploadResponse
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            };
        }
    }

    public async Task<S3ImagesListResponse> ListImagesAsync(string prefix = "uploads/")
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            var images = response.S3Objects.Select(obj => new S3ImageInfo
            {
                Key = obj.Key,
                Url = GenerateS3Url(obj.Key),
                Size = obj.Size,
                LastModified = obj.LastModified.ToString("O")
            }).ToList();

            return new S3ImagesListResponse
            {
                Success = true,
                Images = images
            };
        }
        catch (Exception ex)
        {
            return new S3ImagesListResponse
            {
                Success = false,
                Images = new List<S3ImageInfo>()
            };
        }
    }

    public async Task<string> GetImageUrlAsync(string fileKey)
    {
        try
        {
            // Check if object exists
            await _s3Client.GetObjectMetadataAsync(_bucketName, fileKey);
            
            // Generate presigned URL
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddHours(1),
                Verb = HttpVerb.GET
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch
        {
            throw new FileNotFoundException("Image not found");
        }
    }

    public string GenerateS3Url(string key)
    {
        if (_region == "us-east-1")
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        
        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
    }
}