using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.Configure<PipelineSettings>(builder.Configuration.GetSection("Pipeline"));
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<RawFileService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<StatusService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Load configuration from Functions config file at startup
// This ensures the Functions config is the single source of truth
using (var scope = app.Services.CreateScope())
{
    var configService = scope.ServiceProvider.GetRequiredService<ConfigService>();
    var pipelineSettings = scope.ServiceProvider.GetRequiredService<IOptions<PipelineSettings>>();
    
    try
    {
        // Preload and cache the configuration
        var config = configService.GetCachedConfig();
        
        // Log appropriate message based on how config was loaded
        if (string.IsNullOrWhiteSpace(pipelineSettings.Value.ConfigFile))
        {
            app.Logger.LogInformation("No Functions config file configured. Using default pipeline configuration.");
        }
        else if (!File.Exists(pipelineSettings.Value.ConfigFile))
        {
            app.Logger.LogWarning("Functions config file not found at '{ConfigFile}'. Using default pipeline configuration.", pipelineSettings.Value.ConfigFile);
        }
        else
        {
            app.Logger.LogInformation("Pipeline configuration loaded successfully from Functions config file: {ConfigFile}", pipelineSettings.Value.ConfigFile);
        }
        
        app.Logger.LogInformation("  Input Directory: {InputDirectory}", config.InputDirectory);
        app.Logger.LogInformation("  Output Directory: {OutputDirectory}", config.OutputDirectory);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to load pipeline configuration at startup");
        throw;
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();