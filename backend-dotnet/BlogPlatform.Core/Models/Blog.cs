using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Core.Models;

public class Blog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("title")]
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [BsonElement("content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("main_image_url")]
    public string? MainImageUrl { get; set; }

    [BsonElement("published")]
    public bool Published { get; set; } = true;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("comment_count")]
    public int CommentCount { get; set; } = 0;

    [BsonElement("likes_count")]
    public int LikesCount { get; set; } = 0;
}

public class BlogResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string? MainImageUrl { get; set; }
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
    public int LikesCount { get; set; }
}

public class BlogCreate
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public string? MainImageUrl { get; set; }

    public bool Published { get; set; } = true;
}

public class BlogUpdate
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    public string? Content { get; set; }

    public List<string>? Tags { get; set; }

    public string? MainImageUrl { get; set; }

    public bool? Published { get; set; }
}

public class PaginatedBlogsResponse
{
    public List<BlogResponse> Blogs { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
}