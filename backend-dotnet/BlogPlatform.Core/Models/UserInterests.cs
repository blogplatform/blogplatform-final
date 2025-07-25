using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BlogPlatform.Core.Models;

public class UserInterests
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("interests")]
    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public List<string> Interests { get; set; } = new();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserInterestsResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<string> Interests { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserInterestsCreate
{
    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public List<string> Interests { get; set; } = new();
}

public class UserInterestsUpdate
{
    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public List<string> Interests { get; set; } = new();
}