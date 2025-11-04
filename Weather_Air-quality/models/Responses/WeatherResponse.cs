using Newtonsoft.Json;

namespace weather_condition.Models.Responses;

public record WeatherAndAirQualityResponse
{
    public string City { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TemperatureC { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int AirQualityIndex { get; set; }
    public string AirQualityDescription => GetAirQualityDescription(AirQualityIndex);
    public Pollutants Pollutants { get; set; } = new();
    public string WeatherDescription { get; set; } = string.Empty;

    private static string GetAirQualityDescription(int aqi)
    {
        return aqi switch
        {
            1 => "Good",
            2 => "Fair",
            3 => "Moderate",
            4 => "Poor",
            5 => "Very Poor",
            _ => "Unknown"
        };
    }
}

public class GeoResponse
{
    [JsonProperty("coord")]
    public Coordinates? Coord { get; set; }
}

public class Coordinates
{
    [JsonProperty("lat")]
    public double Lat { get; set; }
    
    [JsonProperty("lon")]
    public double Lon { get; set; }
}

public class WeatherApiResponse
{
    [JsonProperty("main")]
    public MainData? Main { get; set; }
    
    [JsonProperty("wind")]
    public WindData? Wind { get; set; }
    
    [JsonProperty("weather")]
    public List<WeatherDescription>? Weather { get; set; }
}

public class MainData
{
    [JsonProperty("temp")]
    public double Temp { get; set; }
    
    [JsonProperty("humidity")]
    public int Humidity { get; set; }
}

public class WindData
{
    [JsonProperty("speed")]
    public double Speed { get; set; }
}

public class WeatherDescription
{
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
}

public class PollutionResponse
{
    [JsonProperty("list")]
    public List<PollutionData>? List { get; set; }
}

public class PollutionData
{
    [JsonProperty("main")]
    public AirQualityMain? Main { get; set; }
    
    [JsonProperty("components")]
    public Pollutants? Components { get; set; }
}

public class AirQualityMain
{
    [JsonProperty("aqi")]
    public int Aqi { get; set; }
}

public class Pollutants
{
    [JsonProperty("co")]
    public double Co { get; set; }

    [JsonProperty("no")]
    public double No { get; set; }

    [JsonProperty("no2")]
    public double No2 { get; set; }

    [JsonProperty("o3")]
    public double O3 { get; set; }

    [JsonProperty("so2")]
    public double So2 { get; set; }

    [JsonProperty("pm2_5")]
    public double Pm25 { get; set; }

    [JsonProperty("pm10")]
    public double Pm10 { get; set; }

    [JsonProperty("nh3")]
    public double Nh3 { get; set; }
}