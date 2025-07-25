using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ImageUploadResponse>> UploadImage(IFormFile image)
    {
        if (image == null)
        {
            return BadRequest(new ImageUploadResponse 
            { 
                Success = false, 
                Message = "No file provided" 
            });
        }

        var result = await _imageService.UploadImageAsync(image);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("list")]
    public async Task<ActionResult<S3ImagesListResponse>> ListImages([FromQuery] string prefix = "uploads/")
    {
        var result = await _imageService.ListImagesAsync(prefix);
        return Ok(result);
    }

    [HttpGet("{fileKey}")]
    public async Task<ActionResult<ImageUploadResponse>> GetImageUrl(string fileKey)
    {
        try
        {
            var url = await _imageService.GetImageUrlAsync(fileKey);
            return Ok(new ImageUploadResponse 
            { 
                Success = true, 
                ImageUrl = url 
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new ImageUploadResponse 
            { 
                Success = false, 
                Message = "Image not found" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ImageUploadResponse 
            { 
                Success = false, 
                Message = $"Error generating URL: {ex.Message}" 
            });
        }
    }
}