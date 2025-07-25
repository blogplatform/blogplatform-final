using BlogPlatform.Core.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace BlogPlatform.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") 
            ?? throw new InvalidOperationException("MongoDB connection string not found");
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("blogging");
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Blog> Blogs => _database.GetCollection<Blog>("blogs");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");
    public IMongoCollection<Like> Likes => _database.GetCollection<Like>("likes");
    public IMongoCollection<BlogPlatform.Core.Models.Tag> Tags => _database.GetCollection<BlogPlatform.Core.Models.Tag>("tags");
    public IMongoCollection<UserInterests> UserInterests => _database.GetCollection<UserInterests>("user_interests");
}