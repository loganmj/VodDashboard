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

### Development Workflow
1. Make changes in your editor
2. Build to check for compilation errors
3. Run the application to test
4. Use the OpenAPI endpoint (/openapi/v1.json) for API documentation
5. Test endpoints using the generated HTTP file or tools like Postman

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

## AI Assistant Guidelines
When suggesting code or making changes:
1. Maintain consistency with existing code style
2. Consider the VOD processing context
3. Prioritize security for file handling
4. Suggest async patterns for I/O operations
5. Recommend appropriate NuGet packages when needed
6. Follow .NET and ASP.NET Core best practices
7. Consider scalability and performance implications
