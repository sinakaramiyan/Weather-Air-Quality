using System.Net;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using weather_condition.Models.Responses;
using weather_condition.Models.Configuration;
using weather_condition.Services;
using Xunit;

namespace WeatherCondition.Tests;

public class OpenWeatherServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherConfig _config;
    private readonly Mock<ILogger<OpenWeatherService>> _loggerMock;
    private readonly OpenWeatherService _service;

    public OpenWeatherServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _config = new OpenWeatherConfig 
        { 
            ApiKey = "991c85b25ad89b6c8c625f489671fa37",
            BaseUrl = "https://api.openweathermap.org/data/2.5" 
        };
        _loggerMock = new Mock<ILogger<OpenWeatherService>>();
        
        _service = new OpenWeatherService(_httpClient, _config);
    }

    [Fact]
    public async Task GetWeatherAndAirQualityAsync_ValidCity_ReturnsWeatherData()
    {
        // Arrange
        var city = "tehran";
        
        // Mock coordinates response
        var coordinatesResponse = new GeoResponse
        {
            Coord = new Coordinates { Lat = 35.6944, Lon = 51.4215 }
        };

        // Mock weather response
        var weatherResponse = new WeatherApiResponse
        {
            Main = new MainData { Temp = 15.5, Humidity = 65 },
            Wind = new WindData { Speed = 5.2 },
            Weather = new List<WeatherDescription>
            {
                new WeatherDescription { Description = "clear sky" }
            }
        };

        // Mock pollution response
        var pollutionResponse = new PollutionResponse
        {
            List = new List<PollutionData>
            {
                new PollutionData
                {
                    Main = new AirQualityMain { Aqi = 2 },
                    Components = new Pollutants
                    {
                        Co = 250.5,
                        No = 10.1,
                        No2 = 20.2,
                        O3 = 120.3,
                        So2 = 5.5,
                        Pm25 = 15.5,
                        Pm10 = 25.5,
                        Nh3 = 1.5
                    }
                }
            }
        };

        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/weather?q=tehran&appid=991c85b25ad89b6c8c625f489671fa37", coordinatesResponse);
        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/weather?lat=35.6944&lon=51.4215&appid=991c85b25ad89b6c8c625f489671fa37&units=metric", weatherResponse);
        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/air_pollution?lat=35.6944&lon=51.4215&appid=991c85b25ad89b6c8c625f489671fa37", pollutionResponse);

        // Act
        var result = await _service.GetWeatherAndAirQualityAsync(city);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(city, result.City);
        Assert.Equal(35.6944, result.Latitude); 
        Assert.Equal(51.4215, result.Longitude); 
        Assert.Equal(15.5, result.TemperatureC);
        Assert.Equal(65, result.Humidity);
        Assert.Equal(5.2, result.WindSpeed);
        Assert.Equal(2, result.AirQualityIndex);
        Assert.Equal("Fair", result.AirQualityDescription);
        Assert.Equal("clear sky", result.WeatherDescription);
        Assert.Equal(250.5, result.Pollutants.Co);
    }

    [Fact]
    public async Task GetWeatherAndAirQualityAsync_CityNotFound_ThrowsHttpRequestException()
    {
        // Arrange
        var city = "InvalidCity";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri != null && req.RequestUri.ToString().Contains($"weather?q={city}")), // Use the actual city variable
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.GetWeatherAndAirQualityAsync(city));
    }

    [Fact]
    public async Task GetCityCoordinatesAsync_ValidCity_ReturnsCoordinates()
    {
        // Arrange
        var city = "Paris";
        var expectedCoordinates = new GeoResponse
        {
            Coord = new Coordinates { Lat = 48.8566, Lon = 2.3522 }
        };

        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=991c85b25ad89b6c8c625f489671fa37", expectedCoordinates);

        // Act
        var result = await _service.GetCityCoordinatesAsync(city);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(48.8566, result.Lat);
        Assert.Equal(2.3522, result.Lon);
    }

    [Fact]
    public async Task GetWeatherDataAsync_ValidCoordinates_ReturnsWeatherData()
    {
        // Arrange
        var lat = 40.7128;
        var lon = -74.0060;
        var expectedWeather = new WeatherApiResponse
        {
            Main = new MainData { Temp = 22.0, Humidity = 70 },
            Wind = new WindData { Speed = 3.5 },
            Weather = new List<WeatherDescription>
            {
                new WeatherDescription { Description = "partly cloudy" }
            }
        };

        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid=991c85b25ad89b6c8c625f489671fa37&units=metric", expectedWeather);

        // Act
        var result = await _service.GetWeatherDataAsync(lat, lon);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Main);
        Assert.Equal(22.0, result.Main.Temp);
        Assert.Equal(70, result.Main.Humidity);
        Assert.NotNull(result.Wind);
        Assert.Equal(3.5, result.Wind.Speed);
        Assert.NotNull(result.Weather);
        Assert.Equal("partly cloudy", result.Weather.First().Description);
    }

    [Fact]
    public async Task GetAirPollutionDataAsync_ValidCoordinates_ReturnsPollutionData()
    {
        // Arrange
        var lat = 34.0522;
        var lon = -118.2437;
        var expectedPollution = new PollutionResponse
        {
            List = new List<PollutionData>
            {
                new PollutionData
                {
                    Main = new AirQualityMain { Aqi = 3 },
                    Components = new Pollutants { Co = 300.0, Pm25 = 20.0 }
                }
            }
        };

        SetupHttpResponse($"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat}&lon={lon}&appid=991c85b25ad89b6c8c625f489671fa37", expectedPollution);

        // Act
        var result = await _service.GetAirPollutionDataAsync(lat, lon);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.List);
        Assert.NotEmpty(result.List);
        
        var firstPollutionData = result.List[0];
        Assert.NotNull(firstPollutionData);
        Assert.NotNull(firstPollutionData.Main);
        Assert.NotNull(firstPollutionData.Components);
        
        Assert.Equal(3, firstPollutionData.Main.Aqi);
        Assert.Equal(300.0, firstPollutionData.Components.Co);
    }

    private void SetupHttpResponse<T>(string expectedUrl, T responseContent)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri != null && req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(responseContent))
            });
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}