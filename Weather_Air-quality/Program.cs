using System.Net;
using weather_condition.Models.Configuration;
using weather_condition.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration
builder.Configuration.AddEnvironmentVariables();

// Configure OpenWeather settings
var openWeatherConfig = new OpenWeatherConfig
{
    ApiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") 
             ?? throw new InvalidOperationException("OPENWEATHER_API_KEY environment variable is not set"),
    BaseUrl = builder.Configuration["OpenWeather:BaseUrl"] ?? "https://api.openweathermap.org/data/2.5"
};

builder.Services.AddSingleton(openWeatherConfig);

// Configure HttpClient with best practices
builder.Services.AddHttpClient<IOpenWeatherService, OpenWeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "WeatherConditionApp/1.0");
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Weather endpoint with improved error handling
app.MapGet("/weather/{city}", async (string city, IOpenWeatherService weatherService, ILogger<Program> logger) =>
{    
    if (string.IsNullOrWhiteSpace(city))
    {
        return Results.BadRequest("City name is required");
    }

    if (city.Length > 100)
    {
        return Results.BadRequest("City name cannot exceed 100 characters");
    }

    try
    {
        logger.LogInformation("Processing weather request for city: {City}", city);
        var result = await weatherService.GetWeatherAndAirQualityAsync(city);
        return Results.Ok(result);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        logger.LogWarning("City not found: {City}", city);
        return Results.NotFound($"City '{city}' not found");
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "HTTP error fetching weather data for city: {City}", city);
        return Results.Problem($"Error communicating with weather service: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "Invalid operation while processing city: {City}", city);
        return Results.Problem($"Error processing weather data: {ex.Message}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error processing weather request for city: {City}", city);
        return Results.Problem($"An unexpected error occurred: {ex.Message}");
    }
})
.WithName("GetWeatherAndAirQuality")
.WithOpenApi();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Root endpoint to serve HTML file
app.MapGet("/", () =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
    
    if (File.Exists(filePath))
    {
        return Results.File(filePath, "text/html");
    }
    else
    {
        return Results.NotFound("HTML file not found. Please make sure wwwroot/index.html exists.");
    }
});

app.Run();