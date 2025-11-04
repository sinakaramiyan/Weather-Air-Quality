using weather_condition.Models.Responses;

namespace weather_condition.Services;

public interface IOpenWeatherService
{
    Task<WeatherAndAirQualityResponse> GetWeatherAndAirQualityAsync(string city);
    Task<Coordinates> GetCityCoordinatesAsync(string city);
    Task<WeatherApiResponse> GetWeatherDataAsync(double lat, double lon);
    Task<PollutionResponse> GetAirPollutionDataAsync(double lat, double lon);
}