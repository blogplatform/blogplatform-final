using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/blogs")]
public class BlogsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;

    public BlogsController(MongoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BlogResponse>> CreateBlog([FromBody] BlogCreate blogCreate)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Process tags
        var processedTags = blogCreate.Tags.Select(tag => tag.Trim().ToLower()).Where(tag => !string.IsNullOrEmpty(tag)).ToList();

        // Create or update tags in database
        if (processedTags.Any())
        {
            var existingTags = await _context.Tags.Find(t => processedTags.Contains(t.Name.ToLower())).ToListAsync();
            var existingTagNames = existingTags.Select(t => t.Name.ToLower()).ToHashSet();
            var newTags = processedTags.Where(tag => !existingTagNames.Contains(tag.ToLower()))
                                    .Select(tag => new Tag { Name = tag, CreatedAt = DateTime.UtcNow })
                                    .ToList();
            
            if (newTags.Any())
            {
                await _context.Tags.InsertManyAsync(newTags);
            }
        }

        var blog = new Blog
        {
            UserId = currentUser.Id,
            Username = currentUser.Username,
            Title = blogCreate.Title,
            Content = blogCreate.Content,
            Tags = processedTags,
            MainImageUrl = blogCreate.MainImageUrl,
            Published = blogCreate.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Blogs.InsertOneAsync(blog);

        return Ok(new BlogResponse
        {
            Id = blog.Id,
            UserId = blog.UserId,
            Username = blog.Username,
            Title = blog.Title,
            Content = blog.Content,
            Tags = blog.Tags,
            MainImageUrl = blog.MainImageUrl,
            Published = blog.Published,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            CommentCount = blog.CommentCount,
            LikesCount = blog.LikesCount
        });
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedBlogsResponse>> GetBlogs(
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10,
        [FromQuery] bool published_only = true,
        [FromQuery] string? tags = null)
    {
        var filter = Builders<Blog>.Filter.Empty;

        if (published_only)
        {
            filter &= Builders<Blog>.Filter.Eq(b => b.Published, true);
        }

        if (!string.IsNullOrEmpty(tags))
        {
            var tagList = tags.Split(',').Select(t => t.Trim()).ToList();
            filter &= Builders<Blog>.Filter.AnyIn(b => b.Tags, tagList);
        }

        var skip = (page - 1) * page_size;
        var total = await _context.Blogs.CountDocumentsAsync(filter);

        var blogs = await _context.Blogs.Find(filter)
            .Sort(Builders<Blog>.Sort.Descending(b => b.CreatedAt))
            .Skip(skip)
            .Limit(page_size)
            .ToListAsync();

        // Fetch usernames for each blog
        var blogResponses = new List<BlogResponse>();
        foreach (var blog in blogs)
        {
            var author = await _context.Users.Find(u => u.Id == blog.UserId).FirstOrDefaultAsync();
            blogResponses.Add(new BlogResponse
            {
                Id = blog.Id,
                UserId = blog.UserId,
                Username = author?.Username ?? "Unknown",
                Title = blog.Title,
                Content = blog.Content,
                Tags = blog.Tags,
                MainImageUrl = blog.MainImageUrl,
                Published = blog.Published,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                CommentCount = blog.CommentCount,
                LikesCount = blog.LikesCount
            });
        }

        return Ok(new PaginatedBlogsResponse
        {
            Blogs = blogResponses,
            Total = (int)total,
            Page = page,
            Limit = page_size,
            TotalPages = (int)Math.Ceiling((double)total / page_size)
        });
    }

    [HttpGet("my-blogs")]
    [Authorize]
    public async Task<ActionResult<List<BlogResponse>>> GetMyBlogs(
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var skip = (page - 1) * page_size;
        var blogs = await _context.Blogs.Find(b => b.UserId == currentUser.Id)
            .Sort(Builders<Blog>.Sort.Descending(b => b.CreatedAt))
            .Skip(skip)
            .Limit(page_size)
            .ToListAsync();

        var blogResponses = blogs.Select(blog => new BlogResponse
        {
            Id = blog.Id,
            UserId = blog.UserId,
            Username = currentUser.Username,
            Title = blog.Title,
            Content = blog.Content,
            Tags = blog.Tags,
            MainImageUrl = blog.MainImageUrl,
            Published = blog.Published,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            CommentCount = blog.CommentCount,
            LikesCount = blog.LikesCount
        }).ToList();

        return Ok(blogResponses);
    }

    [HttpGet("{blogId}")]
    public async Task<ActionResult<BlogResponse>> GetBlog(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

        var blog = await _context.Blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        if (blog == null)
        {
            return NotFound(new { detail = "Blog not found" });
        }

        // Fetch author's username
        var author = await _context.Users.Find(u => u.Id == blog.UserId).FirstOrDefaultAsync();

        return Ok(new BlogResponse
        {
            Id = blog.Id,
            UserId = blog.UserId,
            Username = author?.Username ?? "Unknown",
            Title = blog.Title,
            Content = blog.Content,
            Tags = blog.Tags,
            MainImageUrl = blog.MainImageUrl,
            Published = blog.Published,
            CreatedAt = blog.CreatedAt,
            UpdatedAt = blog.UpdatedAt,
            CommentCount = blog.CommentCount,
            LikesCount = blog.LikesCount
        });
    }

    [HttpPut("{blogId}")]
    [Authorize]
    public async Task<ActionResult<BlogResponse>> UpdateBlog(string blogId, [FromBody] BlogUpdate blogUpdate)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var blog = await _context.Blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        if (blog == null)
        {
            return NotFound(new { detail = "Blog not found" });
        }

        if (blog.UserId != currentUser.Id)
        {
            return Forbid("You don't have permission to edit this blog");
        }

        // Process tags if provided
        if (blogUpdate.Tags != null)
        {
            var processedTags = blogUpdate.Tags.Select(tag => tag.Trim().ToLower()).Where(tag => !string.IsNullOrEmpty(tag)).ToList();
            
            if (processedTags.Any())
            {
                var existingTags = await _context.Tags.Find(t => processedTags.Contains(t.Name.ToLower())).ToListAsync();
                var existingTagNames = existingTags.Select(t => t.Name.ToLower()).ToHashSet();
                var newTags = processedTags.Where(tag => !existingTagNames.Contains(tag.ToLower()))
                                        .Select(tag => new Tag { Name = tag, CreatedAt = DateTime.UtcNow })
                                        .ToList();
                
                if (newTags.Any())
                {
                    await _context.Tags.InsertManyAsync(newTags);
                }
            }
        }

        var updateBuilder = Builders<Blog>.Update.Set(b => b.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(blogUpdate.Title))
            updateBuilder = updateBuilder.Set(b => b.Title, blogUpdate.Title);

        if (!string.IsNullOrEmpty(blogUpdate.Content))
            updateBuilder = updateBuilder.Set(b => b.Content, blogUpdate.Content);

        if (blogUpdate.Tags != null)
            updateBuilder = updateBuilder.Set(b => b.Tags, blogUpdate.Tags.Select(tag => tag.Trim().ToLower()).Where(tag => !string.IsNullOrEmpty(tag)).ToList());

        if (blogUpdate.MainImageUrl != null)
            updateBuilder = updateBuilder.Set(b => b.MainImageUrl, blogUpdate.MainImageUrl);

        if (blogUpdate.Published.HasValue)
            updateBuilder = updateBuilder.Set(b => b.Published, blogUpdate.Published.Value);

        await _context.Blogs.UpdateOneAsync(b => b.Id == blogId, updateBuilder);

        var updatedBlog = await _context.Blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        var author = await _context.Users.Find(u => u.Id == updatedBlog!.UserId).FirstOrDefaultAsync();

        return Ok(new BlogResponse
        {
            Id = updatedBlog!.Id,
            UserId = updatedBlog.UserId,
            Username = author?.Username ?? "Unknown",
            Title = updatedBlog.Title,
            Content = updatedBlog.Content,
            Tags = updatedBlog.Tags,
            MainImageUrl = updatedBlog.MainImageUrl,
            Published = updatedBlog.Published,
            CreatedAt = updatedBlog.CreatedAt,
            UpdatedAt = updatedBlog.UpdatedAt,
            CommentCount = updatedBlog.CommentCount,
            LikesCount = updatedBlog.LikesCount
        });
    }

    [HttpDelete("{blogId}")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> DeleteBlog(string blogId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(blogId))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var currentUser = await _authService.GetUserByEmailAsync(email);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var blog = await _context.Blogs.Find(b => b.Id == blogId).FirstOrDefaultAsync();
        if (blog == null)
        {
            return NotFound(new { detail = "Blog not found" });
        }

        if (blog.UserId != currentUser.Id)
        {
            return Forbid("You don't have permission to delete this blog");
        }

        // Delete related comments and likes
        await _context.Comments.DeleteManyAsync(c => c.BlogId == blogId);
        await _context.Likes.DeleteManyAsync(l => l.BlogId == blogId);
        await _context.Blogs.DeleteOneAsync(b => b.Id == blogId);

        return Ok(new MessageResponse { Message = "Blog deleted successfully" });
    }

    [HttpGet("search/{query}")]
    public async Task<ActionResult<PaginatedBlogsResponse>> SearchBlogs(
        string query,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10)
    {
        var filter = Builders<Blog>.Filter.And(
            Builders<Blog>.Filter.Eq(b => b.Published, true),
            Builders<Blog>.Filter.Or(
                Builders<Blog>.Filter.Regex(b => b.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<Blog>.Filter.Regex(b => b.Content, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                Builders<Blog>.Filter.AnyIn(b => b.Tags, new[] { query.ToLower() })
            )
        );

        var skip = (page - 1) * page_size;
        var total = await _context.Blogs.CountDocumentsAsync(filter);

        var blogs = await _context.Blogs.Find(filter)
            .Sort(Builders<Blog>.Sort.Descending(b => b.CreatedAt))
            .Skip(skip)
            .Limit(page_size)
            .ToListAsync();

        var blogResponses = new List<BlogResponse>();
        foreach (var blog in blogs)
        {
            var author = await _context.Users.Find(u => u.Id == blog.UserId).FirstOrDefaultAsync();
            blogResponses.Add(new BlogResponse
            {
                Id = blog.Id,
                UserId = blog.UserId,
                Username = author?.Username ?? "Unknown",
                Title = blog.Title,
                Content = blog.Content,
                Tags = blog.Tags,
                MainImageUrl = blog.MainImageUrl,
                Published = blog.Published,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                CommentCount = blog.CommentCount,
                LikesCount = blog.LikesCount
            });
        }

        return Ok(new PaginatedBlogsResponse
        {
            Blogs = blogResponses,
            Total = (int)total,
            Page = page,
            Limit = page_size,
            TotalPages = (int)Math.Ceiling((double)total / page_size)
        });
    }
}