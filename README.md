# Weather-Air-Quality

## 1. Introduction

This document provides an overview of the **.NET Core Web API** developed for the technical assessment. The API fulfills the user story of retrieving comprehensive environmental data for a given city, including **weather conditions** and **air quality information**, by integrating with the **OpenWeatherMap API**.

---

## 2. Project Overview

The solution is a **clean, testable, and scalable** ASP.NET Core Web API built on **.NET 8.0**. It follows modern software development principles, including:

- Separation of concerns
- Dependency injection
- Asynchronous programming

### Key Features

- Fetches and returns **current weather data** (Temperature, Humidity, Wind Speed, Coordinates)
- Fetches and returns **air pollution data** (AQI, Major Pollutants)
- Structured, **JSON-formatted responses**
- Contain api return and UI implementation
- Includes a **unit test** to validate core functionality
- Easy to set up and run with a **single command**

---

## 3. Technology Stack & Libraries

| Category              | Technology/Library |
|-----------------------|--------------------|
| **Framework**         | .NET 8.0           |
| **Testing Frameworks**| xUnit, Moq, FluentAssertions |
| **NuGet Packages**    | `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore`, `Newtonsoft.Json`, `Microsoft.NET.Test.Sdk` |

---
## 4. Key Components Explained

1. **`OpenWeatherConfig`**  
   A configuration class that holds the **API Key** and **Base URL**, loaded from `appsettings.json`. This makes it easy to change settings without code changes.

2. **`WeatherResponse`**  
   A **Data Transfer Object (DTO)** that represents the combined response sent to the client, containing all required fields (Temperature, AQI, Pollutants, etc.).

3. **`IOpenWeatherService` & `OpenWeatherService`**  
   - The `IOpenWeatherService` interface defines the contract for getting weather data.  
   - The `OpenWeatherService` class implements this interface and contains the core logic for:  
     - Making an HTTP call to get the city's **coordinates (Geocoding API)**  
     - Making a subsequent HTTP call to get the **air pollution data** for those coordinates  
     - Combining the data into a unified `WeatherResponse` object

4. **`Program.cs`**  
   Configures the application services (HTTP Client, Configuration, Swagger) and sets up the API endpoint.

5. **`UnitTest1.cs`**  
   Contains a unit test that uses **Moq** to mock the `IOpenWeatherService`, ensuring the **controller logic is correct** without making real API calls.


## 5. API Usage
**Running the Application** 
    1.  Clone the repository. 
    2.  Add your **OpenWeatherMap API key** to the 
        `launchSettings.json` file: 
        Change your api key with "YOUR_ACTUAL_API_KEY_HERE"
    3.  Run the following command in the project root directory: 
        ```bash dotnet run ``` 
    4.  The API will start.
        - **Swagger UI** will be available at: [https://localhost:5266/swagger](https://localhost:7076/swagger) for easy testing.
        - **Graphical UI** will be availabe at: [https://localhost:5266/]
        - **Bsic API** will be availabe at: [https://localhost:5266/weather/{city_name}]