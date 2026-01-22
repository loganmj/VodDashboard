using VodDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<RawFileService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<StatusService>();
builder.Services.AddSingleton<ConfigService>();

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