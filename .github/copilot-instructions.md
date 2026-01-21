# Copilot Instructions for VodDashboard

## Project Overview
VodDashboard is a tool for automatically ingesting and editing VOD (Video on Demand) content. This is an ASP.NET Core Web API project built with .NET 10.0.

## Technology Stack
- **Framework**: .NET 10.0
- **Project Type**: ASP.NET Core Web API
- **Language**: C# with nullable reference types enabled
- **Features**: OpenAPI/Swagger support

## Project Structure
```
VodDashboard/
├── VodDashboard.Api/          # Main Web API project
│   ├── Program.cs             # Application entry point
│   ├── appsettings.json       # Configuration
│   └── VodDashboard.Api.csproj
└── VodDashboard.sln           # Solution file
```

## Coding Standards

### C# Style Guidelines
- Use **implicit usings** (enabled in project)
- Enable **nullable reference types** for all code
- Follow Microsoft's C# coding conventions
- Use modern C# features (records, pattern matching, etc.)
- Prefer minimal APIs over controller-based APIs for simple endpoints
- NEVER use `var` when the type is not obvious from the right-hand side
- NEVER suppress nullable warnings with `!` operator unless absolutely necessary
- ALWAYS use expression-bodied members for simple properties and methods

#### Code Style Examples

**Preferred minimal API endpoint style:**
```csharp
app.MapGet("/api/videos", async (IVideoService service) =>
{
    var videos = await service.GetVideosAsync();
    return Results.Ok(videos);
})
.WithName("GetVideos")
.WithOpenApi();
```

**Preferred record definition:**
```csharp
public record VideoMetadata(
    Guid Id,
    string Title,
    TimeSpan Duration,
    DateTimeOffset CreatedAt);
```

**Preferred async pattern:**
```csharp
public async Task<Result<Video>> ProcessVideoAsync(Guid videoId, CancellationToken cancellationToken)
{
    var video = await _repository.GetByIdAsync(videoId, cancellationToken);
    if (video is null)
    {
        return Result<Video>.NotFound($"Video {videoId} not found");
    }
    
    return Result<Video>.Success(video);
}
```

### Naming Conventions
- **PascalCase** for class names, method names, and properties
- **camelCase** for local variables and parameters
- **PascalCase** for constants
- Use descriptive names that clearly indicate purpose

### Code Organization
- Keep Program.cs clean and organized
- Group related services and configurations together
- Use extension methods for service registration when appropriate
- Separate concerns (routing, middleware, services)

## Development Guidelines

### API Development
- Follow RESTful principles for API design
- Use appropriate HTTP methods (GET, POST, PUT, DELETE, PATCH)
- Return proper HTTP status codes
- Use OpenAPI/Swagger annotations for documentation
- Implement proper error handling and validation

### Dependency Management
- Use NuGet packages from official sources
- Keep dependencies up to date
- Minimize external dependencies when possible
- Document any non-standard packages

### Configuration
- Use appsettings.json for configuration
- Use appsettings.Development.json for development-specific settings
- Never commit secrets or sensitive data
- Use environment variables or user secrets for sensitive configuration

### Error Handling
- Implement proper exception handling
- Return meaningful error messages
- Log errors appropriately
- Use problem details for API errors

## Building and Testing

### Build Commands
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project VodDashboard.Api/VodDashboard.Api.csproj
```

### Testing Guidelines
- Write unit tests for all business logic
- Use integration tests for API endpoints
- Follow AAA pattern (Arrange, Act, Assert) in tests
- Name test methods descriptively: `MethodName_Scenario_ExpectedResult`
- Mock external dependencies using interfaces
- Test both happy paths and error scenarios
- ALWAYS run tests before committing changes

**Example test structure:**
```csharp
public class VideoServiceTests
{
    [Fact]
    public async Task GetVideoAsync_ExistingVideo_ReturnsVideo()
    {
        // Arrange
        var mockRepo = new Mock<IVideoRepository>();
        var expectedVideo = new Video(Guid.NewGuid(), "Test Video");
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(expectedVideo);
        var service = new VideoService(mockRepo.Object);
        
        // Act
        var result = await service.GetVideoAsync(expectedVideo.Id);
        
        // Assert
        Assert.Equal(expectedVideo.Id, result.Id);
    }
}
```

### Development Workflow
1. Make changes in your editor
2. Build to check for compilation errors
3. Run tests to verify functionality
4. Run the application to test manually
5. Use the OpenAPI endpoint (/openapi/v1.json) for API documentation
6. Test endpoints using the generated HTTP file or tools like Postman

## Best Practices

### Security
- Always validate input
- Use HTTPS in production
- Implement authentication and authorization as needed
- Follow OWASP security guidelines
- Keep dependencies updated for security patches

### Performance
- Use async/await for I/O operations
- Minimize database calls
- Use caching when appropriate
- Profile performance-critical sections

### Documentation
- Write XML documentation comments for public APIs
- Keep README.md updated
- Document complex business logic
- Maintain API documentation through OpenAPI

## VOD Dashboard Specific Guidelines

### Domain Context
- This application handles Video on Demand content
- Focus on ingestion and editing workflows
- Consider video file processing requirements
- Plan for scalability with large media files

### Future Considerations
- Video processing may require async/background jobs
- Storage solutions for media files
- Streaming capabilities
- Metadata management for VOD content

## Boundaries and Restrictions

### What NOT to Do
- **NEVER** commit secrets, API keys, or sensitive data to the repository
- **NEVER** use `any` or disable nullable reference types
- **NEVER** catch generic exceptions without proper handling
- **NEVER** use blocking calls (`.Result`, `.Wait()`) in async code
- **NEVER** modify the `.csproj` file without explicit instruction
- **NEVER** remove error handling or validation logic
- **NEVER** hardcode connection strings or file paths
- **NEVER** use deprecated APIs or packages
- **DO NOT** edit or remove existing test files unless fixing actual bugs
- **DO NOT** add dependencies without checking for existing solutions
- **DO NOT** bypass existing validation or security measures

### Files to Avoid Modifying
- `.github/workflows/*` - CI/CD configuration (modify only when explicitly requested)
- `LICENSE` - Project license
- `.gitignore` - Git ignore rules (modify only when adding new artifacts)

## AI Assistant Guidelines
When suggesting code or making changes:
1. **ALWAYS** maintain consistency with existing code style
2. **ALWAYS** consider the VOD processing context
3. **ALWAYS** prioritize security for file handling
4. **ALWAYS** use async patterns for I/O operations
5. **ALWAYS** validate input and handle errors properly
6. **ALWAYS** check existing dependencies before adding new ones
7. Recommend appropriate NuGet packages when needed
8. Follow .NET and ASP.NET Core best practices
9. Consider scalability and performance implications
10. Write clear, self-documenting code with minimal comments
11. Use meaningful variable names that express intent
12. Keep methods small and focused on a single responsibility
