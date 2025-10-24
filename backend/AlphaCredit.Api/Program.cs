using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using AlphaCredit.Api.Services;
using System.Text.Json.Serialization;
using QuestPDF.Infrastructure;

// Configurar licencia de QuestPDF para uso comunitario
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar para manejar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContext<AlphaCreditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AlphaCreditDb")));

// Add Configuration Options
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<CurrencySettings>(
    builder.Configuration.GetSection("Currency"));

// Add Application Services
builder.Services.AddScoped<FechaSistemaService>();
builder.Services.AddScoped<PrestamoAmortizacionService>();
builder.Services.AddScoped<PrestamoMoraService>();
builder.Services.AddScoped<PrestamoMoraCalculoService>();
builder.Services.AddScoped<PrestamoAbonoService>();
builder.Services.AddScoped<PrestamoEstadoCuentaService>();
builder.Services.AddScoped<ReciboAbonoService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
