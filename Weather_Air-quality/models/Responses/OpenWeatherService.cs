using System.Net;
using Newtonsoft.Json;
using weather_condition.Models.Configuration;
using weather_condition.Models.Responses;

namespace weather_condition.Services;

public class OpenWeatherService : IOpenWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherConfig _config;

    public OpenWeatherService(HttpClient httpClient, OpenWeatherConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<WeatherAndAirQualityResponse> GetWeatherAndAirQualityAsync(string city)
    {
        var coordinates = await GetCityCoordinatesAsync(city);
        var weatherData = await GetWeatherDataAsync(coordinates.Lat, coordinates.Lon);
        var pollutionData = await GetAirPollutionDataAsync(coordinates.Lat, coordinates.Lon);

        return new WeatherAndAirQualityResponse
        {
            City = city,
            Latitude = coordinates.Lat,
            Longitude = coordinates.Lon,
            TemperatureC = weatherData?.Main?.Temp ?? 0,
            Humidity = weatherData?.Main?.Humidity ?? 0,
            WindSpeed = weatherData?.Wind?.Speed ?? 0,
            AirQualityIndex = pollutionData?.List?.FirstOrDefault()?.Main?.Aqi ?? 0,
            Pollutants = pollutionData?.List?.FirstOrDefault()?.Components ?? new Pollutants(),
            WeatherDescription = weatherData?.Weather?.FirstOrDefault()?.Description ?? "Unknown"
        };
    }

    public async Task<Coordinates> GetCityCoordinatesAsync(string city)
    {
        var geoUrl = $"{_config.BaseUrl}/weather?q={WebUtility.UrlEncode(city)}&appid={_config.ApiKey}";
        var response = await _httpClient.GetAsync(geoUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"City '{city}' not found", null, response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync();
        var geoData = JsonConvert.DeserializeObject<GeoResponse>(content);
        
        return geoData?.Coord ?? throw new InvalidOperationException("Failed to get coordinates for the city");
    }

    public async Task<WeatherApiResponse> GetWeatherDataAsync(double lat, double lon)
    {
        var weatherUrl = $"{_config.BaseUrl}/weather?lat={lat}&lon={lon}&appid={_config.ApiKey}&units=metric";
        var response = await _httpClient.GetAsync(weatherUrl);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<WeatherApiResponse>(content) 
               ?? throw new InvalidOperationException("Failed to deserialize weather data");
    }

    public async Task<PollutionResponse> GetAirPollutionDataAsync(double lat, double lon)
    {
        var pollutionUrl = $"{_config.BaseUrl}/air_pollution?lat={lat}&lon={lon}&appid={_config.ApiKey}";
        var response = await _httpClient.GetAsync(pollutionUrl);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<PollutionResponse>(content) 
               ?? throw new InvalidOperationException("Failed to deserialize pollution data");
    }
}