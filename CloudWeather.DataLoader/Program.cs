
using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .AddEnvironmentVariables()
    .Build();

//var services = config.GetSection("Services");

Services configServices = config.GetRequiredSection("Services").Get<Services>();

var temperatureServiceHost = configServices.Temperature.Host;
var temperatureServicePort = configServices.Temperature.Port;


var precipitationServiceHost = configServices.Precipitation.Host;
var precipitationServicePort = configServices.Precipitation.Port;

var zipCodes=new List<string> { 
    "03079",
    "02116",
    "02103",
    "73056",
    "04401",
    "32808",
    "19717",
    "01116"
};

Console.WriteLine("Starting Data Load");

var temperatureHttpClient=new HttpClient();

temperatureHttpClient.BaseAddress = new Uri($"http://{temperatureServiceHost}:{temperatureServicePort}");

var precipitationHttpClient = new HttpClient();

precipitationHttpClient.BaseAddress = new Uri($"http://{precipitationServiceHost}:{precipitationServicePort}");


foreach (var zip in zipCodes) {
    Console.WriteLine($"Processing Zip Code: {zip}");
    var from = DateTime.Now.AddYears(-2);
    var thru = DateTime.Now;

    for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1)) {
        var temps = PostTemperature(zip, day, temperatureHttpClient);
        postPrecipitation(temps[0], zip, day, precipitationHttpClient);
    }
}

void postPrecipitation(int lowTemp, string zip, DateTime day, HttpClient precipitationHttpClient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1;
    PrecipitationModel precipitation;
    if (isPrecip)
    {
        var precipInches = rand.Next(1, 16);
        if (lowTemp < 32)
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "snow",
                ZipCode = zip,
                CreateOn = day
            };
        }
        else
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "rain",
                ZipCode = zip,
                CreateOn = day
            };
        }
    }
    else {
        precipitation = new PrecipitationModel
        {
            AmountInches = 0,
            WeatherType = "none",
            ZipCode = zip,
            CreateOn = day
        };
    }
    var precipitationResponse = precipitationHttpClient.
        PostAsJsonAsync("observation", precipitation)
        .Result;
    if (precipitationResponse.IsSuccessStatusCode) {
        Console.WriteLine($"Posted Precipitation: Date:{day:d}"+
            $"Zip: {zip}"+
             $"Type: {precipitation.WeatherType}" +
              $"Amount (in.): {precipitation.AmountInches}"
            );
    }
}

List<int> PostTemperature(string zip, DateTime day, HttpClient temperatureHttpClient)
{
    var rand=new Random();
    var t1 = rand.Next(0, 100);
    var t2 = rand.Next(0, 100);
    var highLowTemps=new List<int> { t1, t2};
    highLowTemps.Sort();

    TemperatureModel temperature = new TemperatureModel
    {
        TempHighF=highLowTemps[1],
        TempLowF=highLowTemps[0],
        ZipCode=zip,
        CreatedOn = day
    };

    var temperatureResponse = temperatureHttpClient
        .PostAsJsonAsync("observation", temperature)
        .Result;
    if (temperatureResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Precipitation: Date:{day:d}" +
            $"Zip: {zip}" +
             $"High Temperature: {temperature.TempHighF}" +
              $"Low Temperature: {temperature.TempLowF}"
            );
    }
    else { 
        Console.WriteLine(temperatureResponse.ToString());
    }
    return highLowTemps;
}