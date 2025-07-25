using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using BlogPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/tags")]
public class TagsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IAuthService _authService;

    public TagsController(MongoDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> CreateTags([FromBody] List<string> tagNames)
    {
        if (!tagNames.Any())
        {
            return BadRequest(new { detail = "Tag list cannot be empty" });
        }

        var processedTagNames = tagNames.Select(name => name.ToLower()).ToList();

        var existingTags = await _context.Tags.Find(t => processedTagNames.Contains(t.Name.ToLower())).ToListAsync();
        var existingNames = existingTags.Select(t => t.Name.ToLower()).ToHashSet();

        var tagsToInsert = processedTagNames
            .Where(name => !existingNames.Contains(name.ToLower()))
            .Select(name => new Tag { Name = name, CreatedAt = DateTime.UtcNow })
            .ToList();

        if (!tagsToInsert.Any())
        {
            return BadRequest(new { detail = "All tags already exist" });
        }

        await _context.Tags.InsertManyAsync(tagsToInsert);

        return Ok(new MessageResponse { Message = "Tags created successfully" });
    }

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetAllTags(
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 50)
    {
        var tags = await _context.Tags.Find(_ => true)
            .Sort(Builders<Tag>.Sort.Ascending(t => t.Name))
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var tagResponses = tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name
        }).ToList();

        return Ok(tagResponses);
    }

    [HttpGet("search/{query}")]
    public async Task<ActionResult<List<TagResponse>>> SearchTags(
        string query,
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 20)
    {
        var filter = Builders<Tag>.Filter.Regex(t => t.Name, new MongoDB.Bson.BsonRegularExpression(query, "i"));

        var tags = await _context.Tags.Find(filter)
            .Sort(Builders<Tag>.Sort.Ascending(t => t.Name))
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        var tagResponses = tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name
        }).ToList();

        return Ok(tagResponses);
    }

    [HttpGet("{tagId}")]
    public async Task<ActionResult<TagResponse>> GetTag(string tagId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(tagId))
        {
            return BadRequest(new { detail = "Invalid tag ID" });
        }

        var tag = await _context.Tags.Find(t => t.Id == tagId).FirstOrDefaultAsync();
        if (tag == null)
        {
            return NotFound(new { detail = "Tag not found" });
        }

        return Ok(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name
        });
    }

    [HttpDelete("{tagId}")]
    [Authorize]
    public async Task<ActionResult<MessageResponse>> DeleteTag(string tagId)
    {
        if (!MongoDB.Bson.ObjectId.IsValid(tagId))
        {
            return BadRequest(new { detail = "Invalid tag ID" });
        }

        var tag = await _context.Tags.Find(t => t.Id == tagId).FirstOrDefaultAsync();
        if (tag == null)
        {
            return NotFound(new { detail = "Tag not found" });
        }

        // Remove tag from all blogs
        var blogUpdate = Builders<Blog>.Update.Pull(b => b.Tags, tag.Name);
        await _context.Blogs.UpdateManyAsync(_ => true, blogUpdate);

        await _context.Tags.DeleteOneAsync(t => t.Id == tagId);

        return Ok(new MessageResponse { Message = "Tag deleted successfully" });
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<object>>> GetPopularTags([FromQuery] int limit = 10)
    {
        var pipeline = new[]
        {
            new BsonDocument("$unwind", "$tags"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$tags" },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("count", -1)),
            new BsonDocument("$limit", limit),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "tags" },
                { "localField", "_id" },
                { "foreignField", "name" },
                { "as", "tag_info" }
            }),
            new BsonDocument("$unwind", "$tag_info"),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", new BsonDocument("$toString", "$tag_info._id") },
                { "name", "$tag_info.name" },
                { "usage_count", "$count" }
            })
        };

        var result = await _context.Blogs.Aggregate<BsonDocument>(pipeline).ToListAsync();
        
        var popularTags = result.Select(doc => new
        {
            _id = doc["_id"].AsString,
            name = doc["name"].AsString,
            usage_count = doc["usage_count"].AsInt32
        }).ToList();

        return Ok(popularTags);
    }
}