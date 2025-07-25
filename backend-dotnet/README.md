# Blog Platform API - ASP.NET Core

A comprehensive blogging platform backend built with ASP.NET Core 8, MongoDB, and JWT authentication. This is a migration from the original FastAPI Python backend, maintaining full compatibility with the existing Angular frontend.

## Features

- **User Authentication**: JWT-based authentication with access and refresh tokens
- **Blog Management**: Create, read, update, delete blog posts
- **Comments System**: Add and manage comments on blog posts
- **Like/Dislike System**: Like or dislike blog posts
- **Tag System**: Categorize blogs with tags
- **Image Management**: Handle multiple images per blog post with AWS S3
- **Search Functionality**: Search blogs by title and content
- **Cookie-based Refresh Tokens**: Secure refresh token storage in HTTP-only cookies
- **User Interests**: Personalized content recommendations
- **Dashboard Analytics**: Real-time analytics and insights

## Technology Stack

- **ASP.NET Core 8**: Modern, cross-platform web framework
- **MongoDB**: NoSQL database with official C# driver
- **JWT**: JSON Web Tokens for authentication
- **BCrypt.Net**: Password hashing
- **AWS S3**: Cloud storage for images
- **SignalR**: Real-time communication
- **Serilog**: Structured logging
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation

## Project Structure

```
BlogPlatform/
├── BlogPlatform.sln                 # Solution file
├── BlogPlatform.API/                # Web API project
│   ├── Controllers/                 # API controllers
│   ├── Program.cs                   # Application entry point
│   ├── appsettings.json            # Configuration
│   └── BlogPlatform.API.csproj     # Project file
├── BlogPlatform.Core/               # Domain models
│   ├── Models/                      # Entity models
│   └── BlogPlatform.Core.csproj    # Project file
├── BlogPlatform.Infrastructure/     # Data access & services
│   ├── Data/                        # Database context
│   ├── Services/                    # Business services
│   └── BlogPlatform.Infrastructure.csproj
└── README.md                        # This file
```

## Setup Instructions

### 1. Prerequisites

- .NET 8 SDK
- MongoDB (local or cloud)
- AWS Account (for S3 storage)
- Visual Studio 2022 or VS Code

### 2. Clone and Build

```bash
git clone <repository-url>
cd backend-dotnet
dotnet restore
dotnet build
```

### 3. Configuration

Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://username:password@cluster.mongodb.net/blogging"
  },
  "JWT": {
    "SecretKey": "your-super-secret-key-here",
    "AccessTokenExpireMinutes": "30",
    "RefreshTokenExpireDays": "15"
  },
  "AWS": {
    "AccessKey": "your-aws-access-key",
    "SecretKey": "your-aws-secret-key",
    "Region": "eu-north-1",
    "S3BucketName": "your-bucket-name"
  }
}
```

### 4. Run the Application

```bash
cd BlogPlatform.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register a new user
- `POST /api/v1/auth/login` - Login user
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - Logout user
- `GET /api/v1/auth/me` - Get current user profile
- `PUT /api/v1/auth/update-username` - Update username
- `POST /api/v1/auth/change-password` - Change password

### Blogs
- `POST /api/v1/blogs` - Create a new blog post
- `GET /api/v1/blogs` - Get all published blogs (with pagination)
- `GET /api/v1/blogs/my-blogs` - Get current user's blogs
- `GET /api/v1/blogs/{blogId}` - Get specific blog post
- `PUT /api/v1/blogs/{blogId}` - Update blog post
- `DELETE /api/v1/blogs/{blogId}` - Delete blog post
- `GET /api/v1/blogs/search/{query}` - Search blogs

### Comments
- `POST /api/v1/comments/blogs/{blogId}` - Add comment to blog
- `GET /api/v1/comments/blogs/{blogId}` - Get blog comments
- `GET /api/v1/comments/my-comments` - Get user's comments
- `PUT /api/v1/comments/{commentId}` - Update comment
- `DELETE /api/v1/comments/{commentId}` - Delete comment

### Likes
- `POST /api/v1/likes/blogs/{blogId}` - Like/dislike a blog
- `GET /api/v1/likes/blogs/{blogId}/count` - Get like count
- `GET /api/v1/likes/blogs/{blogId}/my-like` - Get user's like status
- `DELETE /api/v1/likes/blogs/{blogId}` - Remove like
- `GET /api/v1/likes/my-likes` - Get user's likes

### Tags
- `POST /api/v1/tags` - Create new tags
- `GET /api/v1/tags` - Get all tags
- `GET /api/v1/tags/search/{query}` - Search tags
- `GET /api/v1/tags/{tagId}` - Get specific tag
- `DELETE /api/v1/tags/{tagId}` - Delete tag
- `GET /api/v1/tags/popular` - Get popular tags

### User Interests
- `POST /api/v1/interests` - Create user interests
- `GET /api/v1/interests` - Get user interests
- `PUT /api/v1/interests` - Update user interests
- `PATCH /api/v1/interests/add` - Add single interest
- `PATCH /api/v1/interests/remove` - Remove single interest
- `DELETE /api/v1/interests` - Delete all interests
- `GET /api/v1/interests/suggestions` - Get interest suggestions

### Images
- `POST /api/v1/images/upload` - Upload image to S3
- `GET /api/v1/images/list` - List uploaded images
- `GET /api/v1/images/{fileKey}` - Get image URL

### Dashboard
- `GET /api/v1/dashboard/totals` - Get total counts
- `GET /api/v1/dashboard/posts-over-time` - Posts analytics
- `GET /api/v1/dashboard/users-over-time` - Users analytics
- `GET /api/v1/dashboard/posts-by-category` - Category analytics
- `GET /api/v1/dashboard/top-tags` - Popular tags
- `GET /api/v1/dashboard/most-liked` - Most liked posts

## Authentication Flow

1. **Register/Login**: User registers or logs in
2. **Token Generation**: Server generates access token and refresh token
3. **Cookie Storage**: Refresh token is stored in HTTP-only cookie
4. **API Access**: Use access token in Authorization header: `Bearer <access_token>`
5. **Token Refresh**: When access token expires, use `/auth/refresh` endpoint
6. **Logout**: Clear refresh token cookie

## Database Schema

The application uses the same MongoDB collections as the original Python backend:

- **users**: User accounts and authentication
- **blogs**: Blog posts with content and metadata
- **comments**: User comments on blog posts
- **likes**: User likes/dislikes on blog posts
- **tags**: Available tags for categorization
- **user_interests**: User interest preferences

## Security Features

- **Password Hashing**: BCrypt for secure password storage
- **JWT Tokens**: Secure access tokens with expiration
- **HTTP-only Cookies**: Refresh tokens stored securely
- **CORS Configuration**: Configurable cross-origin requests
- **Input Validation**: Model validation with data annotations
- **Authorization**: Role-based access control

## Development

### Running in Development

```bash
cd BlogPlatform.API
dotnet watch run
```

### Testing the API

Use Swagger UI at `https://localhost:5001/swagger` to test all endpoints.

### Adding New Features

1. Add models to `BlogPlatform.Core/Models/`
2. Add services to `BlogPlatform.Infrastructure/Services/`
3. Add controllers to `BlogPlatform.API/Controllers/`
4. Update dependency injection in `Program.cs`

## Deployment

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlogPlatform.API/BlogPlatform.API.csproj", "BlogPlatform.API/"]
COPY ["BlogPlatform.Core/BlogPlatform.Core.csproj", "BlogPlatform.Core/"]
COPY ["BlogPlatform.Infrastructure/BlogPlatform.Infrastructure.csproj", "BlogPlatform.Infrastructure/"]
RUN dotnet restore "BlogPlatform.API/BlogPlatform.API.csproj"
COPY . .
WORKDIR "/src/BlogPlatform.API"
RUN dotnet build "BlogPlatform.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogPlatform.API.dll"]
```

### Production Configuration

1. Set secure JWT secret key
2. Configure production MongoDB connection
3. Set up proper AWS credentials
4. Configure HTTPS certificates
5. Set up logging and monitoring

## Migration Notes

This ASP.NET Core backend is a direct migration from the original FastAPI Python backend. Key compatibility points:

- **API Endpoints**: All endpoints maintain the same URL structure and HTTP methods
- **Request/Response Models**: JSON structures are identical to the Python version
- **Authentication**: JWT token format and cookie handling are compatible
- **Database Schema**: Uses the same MongoDB collections and document structure
- **Error Responses**: HTTP status codes and error messages match the original

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is open source and available under the [MIT License](LICENSE).