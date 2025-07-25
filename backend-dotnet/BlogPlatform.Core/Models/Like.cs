using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogPlatform.Core.Models;

public class Like
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("blog_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BlogId { get; set; } = string.Empty;

    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LikeResponse
{
    public string Id { get; set; } = string.Empty;
    public string BlogId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}