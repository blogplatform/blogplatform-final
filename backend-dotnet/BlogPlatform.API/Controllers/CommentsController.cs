using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/comments")]
public class CommentsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;

    public CommentsController(MongoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("blogs/{blogId}")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> CreateComment(string blogId, [FromBody] CommentCreate commentCreate)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(blogId, out _))
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

        var comment = new Comment
        {
            BlogId = blogId,
            UserId = currentUser.Id,
            UserName = currentUser.Username,
            Text = commentCreate.Text,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Comments.InsertOneAsync(comment);

        // Update blog comment count
        var update = Builders<Blog>.Update.Inc(b => b.CommentCount, 1);
        await _context.Blogs.UpdateOneAsync(b => b.Id == blogId, update);

        return Ok(new CommentResponse
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }

    [HttpGet("blogs/{blogId}")]
    public async Task<ActionResult<List<CommentResponse>>> GetBlogComments(
        string blogId,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 20)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(blogId, out _))
        {
            return BadRequest(new { detail = "Invalid blog ID" });
        }

        var comments = await _context.Comments.Find(c => c.BlogId == blogId)
            .Sort(Builders<Comment>.Sort.Descending(c => c.CreatedAt))
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var commentResponses = comments.Select(comment => new CommentResponse
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        }).ToList();

        return Ok(commentResponses);
    }

    [HttpGet("my-comments")]
    [Authorize]
    public async Task<ActionResult<List<CommentResponse>>> GetMyComments(
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 20)
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

        var comments = await _context.Comments.Find(c => c.UserId == currentUser.Id)
            .Sort(Builders<Comment>.Sort.Descending(c => c.CreatedAt))
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var commentResponses = comments.Select(comment => new CommentResponse
        {
            Id = comment.Id,
            BlogId = comment.BlogId,
            UserId = comment.UserId,
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        }).ToList();

        return Ok(commentResponses);
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<ActionResult<CommentResponse>> UpdateComment(string commentId, [FromBody] CommentCreate commentUpdate)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(commentId, out _))
        {
            return BadRequest(new { detail = "Invalid comment ID" });
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

        var comment = await _context.Comments.Find(c => c.Id == commentId && c.UserId == currentUser.Id).FirstOrDefaultAsync();
        if (comment == null)
        {
            return NotFound(new { detail = "Comment not found or permission denied" });
        }

        var update = Builders<Comment>.Update
            .Set(c => c.Text, commentUpdate.Text)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _context.Comments.UpdateOneAsync(c => c.Id == commentId, update);

        var updatedComment = await _context.Comments.Find(c => c.Id == commentId).FirstOrDefaultAsync();

        return Ok(new CommentResponse
        {
            Id = updatedComment!.Id,
            BlogId = updatedComment.BlogId,
            UserId = updatedComment.UserId,
            UserName = updatedComment.UserName,
            Text = updatedComment.Text,
            CreatedAt = updatedComment.CreatedAt,
            UpdatedAt = updatedComment.UpdatedAt
        });
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> DeleteComment(string commentId)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(commentId, out _))
        {
            return BadRequest(new { detail = "Invalid comment ID" });
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

        var comment = await _context.Comments.Find(c => c.Id == commentId).FirstOrDefaultAsync();
        if (comment == null)
        {
            return NotFound(new { detail = "Comment not found" });
        }

        if (comment.UserId != currentUser.Id)
        {
            return Forbid("You don't have permission to delete this comment");
        }

        // Update blog comment count
        var blogUpdate = Builders<Blog>.Update.Inc(b => b.CommentCount, -1);
        await _context.Blogs.UpdateOneAsync(b => b.Id == comment.BlogId, blogUpdate);

        await _context.Comments.DeleteOneAsync(c => c.Id == commentId);

        return Ok(new MessageResponse { Message = "Comment deleted successfully" });
    }
}