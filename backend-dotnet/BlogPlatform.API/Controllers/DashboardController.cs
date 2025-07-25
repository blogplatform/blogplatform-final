using BlogPlatform.Core.Models;
using BlogPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly MongoDbContext _context;

    public DashboardController(MongoDbContext context)
    {
        _context = context;
    }

    [HttpGet("totals")]
    public async Task<ActionResult<DashboardTotals>> GetTotals()
    {
        var totalPosts = await _context.Blogs.CountDocumentsAsync(_ => true);
        var totalUsers = await _context.Users.CountDocumentsAsync(_ => true);
        var totalLikes = await _context.Likes.CountDocumentsAsync(_ => true);
        var totalComments = await _context.Comments.CountDocumentsAsync(_ => true);

        return Ok(new DashboardTotals
        {
            TotalPosts = (int)totalPosts,
            TotalUsers = (int)totalUsers,
            TotalLikes = (int)totalLikes,
            TotalComments = (int)totalComments
        });
    }

    [HttpGet("posts-over-time")]
    public async Task<ActionResult<PostsOverTime>> GetPostsOverTime([FromQuery] string range = "all")
    {
        var today = DateTime.UtcNow;
        DateTime startDate;
        string groupFormat;
        string groupBy;

        switch (range)
        {
            case "7d":
                startDate = today.AddDays(-7);
                groupFormat = "%Y-%m-%d";
                groupBy = "day";
                break;
            case "6m":
                startDate = today.AddMonths(-6);
                groupFormat = "%Y-%m";
                groupBy = "month";
                break;
            case "1y":
                startDate = today.AddYears(-1);
                groupFormat = "%Y-%m";
                groupBy = "month";
                break;
            default:
                var firstBlog = await _context.Blogs.Find(_ => true).Sort(Builders<Blog>.Sort.Ascending(b => b.CreatedAt)).FirstOrDefaultAsync();
                var lastBlog = await _context.Blogs.Find(_ => true).Sort(Builders<Blog>.Sort.Descending(b => b.CreatedAt)).FirstOrDefaultAsync();
                
                if (firstBlog == null || lastBlog == null)
                {
                    return Ok(new PostsOverTime { Labels = new List<string>(), Counts = new List<int>(), GroupBy = "none" });
                }
                
                startDate = firstBlog.CreatedAt;
                var totalDays = (lastBlog.CreatedAt - startDate).Days;
                
                if (totalDays < 31)
                {
                    groupFormat = "%Y-%m-%d";
                    groupBy = "day";
                }
                else if (totalDays < 365)
                {
                    groupFormat = "%Y-%m";
                    groupBy = "month";
                }
                else
                {
                    groupFormat = "%Y";
                    groupBy = "year";
                }
                break;
        }

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("created_at", new BsonDocument("$gte", startDate))),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument("$dateToString", new BsonDocument
                    {
                        { "format", groupFormat },
                        { "date", "$created_at" }
                    })
                },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var results = await _context.Blogs.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var labels = results.Select(r => r["_id"].AsString).ToList();
        var counts = results.Select(r => r["count"].AsInt32).ToList();

        return Ok(new PostsOverTime { Labels = labels, Counts = counts, GroupBy = groupBy });
    }

    [HttpGet("users-over-time")]
    public async Task<ActionResult<UsersOverTime>> GetUsersOverTime([FromQuery] string range = "all")
    {
        var today = DateTime.UtcNow;
        DateTime startDate;
        string groupFormat;
        string groupBy;

        switch (range)
        {
            case "7d":
                startDate = today.AddDays(-7);
                groupFormat = "%Y-%m-%d";
                groupBy = "day";
                break;
            case "6m":
                startDate = today.AddMonths(-6);
                groupFormat = "%Y-%m";
                groupBy = "month";
                break;
            case "1y":
                startDate = today.AddYears(-1);
                groupFormat = "%Y-%m";
                groupBy = "month";
                break;
            default:
                var firstUser = await _context.Users.Find(_ => true).Sort(Builders<User>.Sort.Ascending(u => u.CreatedAt)).FirstOrDefaultAsync();
                var lastUser = await _context.Users.Find(_ => true).Sort(Builders<User>.Sort.Descending(u => u.CreatedAt)).FirstOrDefaultAsync();
                
                if (firstUser == null || lastUser == null)
                {
                    return Ok(new UsersOverTime { Labels = new List<string>(), Counts = new List<int>(), GroupBy = "none" });
                }
                
                startDate = firstUser.CreatedAt;
                var totalDays = (lastUser.CreatedAt - startDate).Days;
                
                if (totalDays < 31)
                {
                    groupFormat = "%Y-%m-%d";
                    groupBy = "day";
                }
                else if (totalDays < 365)
                {
                    groupFormat = "%Y-%m";
                    groupBy = "month";
                }
                else
                {
                    groupFormat = "%Y";
                    groupBy = "year";
                }
                break;
        }

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("created_at", new BsonDocument("$gte", startDate))),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument("$dateToString", new BsonDocument
                    {
                        { "format", groupFormat },
                        { "date", "$created_at" }
                    })
                },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("_id", 1))
        };

        var results = await _context.Users.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var labels = results.Select(r => r["_id"].AsString).ToList();
        var counts = results.Select(r => r["count"].AsInt32).ToList();

        return Ok(new UsersOverTime { Labels = labels, Counts = counts, GroupBy = groupBy });
    }

    [HttpGet("posts-by-category")]
    public async Task<ActionResult<List<PostsByCategory>>> GetPostsByCategory()
    {
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$tags" },
                { "count", new BsonDocument("$sum", 1) }
            })
        };

        var results = await _context.Blogs.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var formatted = new List<PostsByCategory>();
        foreach (var item in results)
        {
            var tags = item["_id"].AsBsonArray;
            var count = item["count"].AsInt32;
            
            foreach (var tag in tags)
            {
                formatted.Add(new PostsByCategory { Name = tag.AsString, Count = count });
            }
        }

        return Ok(formatted);
    }

    [HttpGet("top-tags")]
    public async Task<ActionResult<List<TopTags>>> GetTopTags()
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
            new BsonDocument("$limit", 20)
        };

        var results = await _context.Blogs.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var topTags = results.Select(r => new TopTags
        {
            Name = r["_id"].AsString,
            Value = r["count"].AsInt32
        }).ToList();

        return Ok(topTags);
    }

    [HttpGet("most-liked")]
    public async Task<ActionResult<List<MostLiked>>> GetMostLiked()
    {
        var blogs = await _context.Blogs.Find(_ => true)
            .Sort(Builders<Blog>.Sort.Descending(b => b.LikesCount))
            .Limit(5)
            .ToListAsync();

        var mostLiked = blogs.Select(b => new MostLiked
        {
            Title = b.Title,
            Likes = b.LikesCount
        }).ToList();

        return Ok(mostLiked);
    }
}