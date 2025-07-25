namespace BlogPlatform.Core.Models;

public class DashboardTotals
{
    public int TotalPosts { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
}

public class PostsOverTime
{
    public List<string> Labels { get; set; } = new();
    public List<int> Counts { get; set; } = new();
    public string GroupBy { get; set; } = string.Empty;
}

public class UsersOverTime
{
    public List<string> Labels { get; set; } = new();
    public List<int> Counts { get; set; } = new();
    public string GroupBy { get; set; } = string.Empty;
}

public class PostsByCategory
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopTags
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class MostLiked
{
    public string Title { get; set; } = string.Empty;
    public int Likes { get; set; }
}