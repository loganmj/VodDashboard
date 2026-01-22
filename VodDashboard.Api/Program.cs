using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.Configure<PipelineSettings>(builder.Configuration.GetSection("Pipeline"));
builder.Services.AddSingleton<RawFileService>();
builder.Services.AddSingleton<JobService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();