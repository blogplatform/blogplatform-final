namespace BlogPlatform.Core.Models;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "bearer";
    public int ExpiresIn { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfo User { get; set; } = new();
}

public class RefreshResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "bearer";
    public int ExpiresIn { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}