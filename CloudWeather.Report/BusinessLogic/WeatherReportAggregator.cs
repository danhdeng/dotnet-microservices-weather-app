using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;

namespace CloudWeather.Report.BusinessLogic
{
    public interface IWeatherReportAggregator {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public Task<WeatherReport> BuildReport(string zip, int? days);
    }
    public class WeatherReportAggregator : IWeatherReportAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(
            IHttpClientFactory http,
            ILogger<WeatherReportAggregator> logger,
            IOptions<WeatherDataConfig> weatherDataConfig,
            WeatherReportDbContext db) {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig.Value;
            _db = db;
        }
        public async Task<WeatherReport> BuildReport(string zip, int? days) { 
            var httpClient=_http.CreateClient();
            var precipData = await fetchPrecipitationData(httpClient, zip, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days: " +
                $"total snow: {totalSnow}, rain: {totalRain}"
                );
            var temperatureData = await fetchTemperatureData(httpClient, zip, days);
            var averageTempHigh = temperatureData.Average(t => t.TempHighF);
            var averageTempLow = temperatureData.Average(t => t.TempLowF);
            _logger.LogInformation(
              $"zip: {zip} over last {days} days: " +
              $"Average Temperature High: {averageTempHigh}, Average Temperature Low: {averageTempLow}"
              );

            var weatherReport = new WeatherReport {
                AverageHighF = Math.Round(averageTempHigh, 1),
                AverageLowF =Math.Round( averageTempLow,1),
                RainfallTotalInches=totalRain,
                SnowTotalInches=totalSnow,
                ZipCode=zip,
                CreatedOn=DateTime.UtcNow,
            };
            _db.Add(weatherReport);
            await _db.SaveChangesAsync();
            return weatherReport;
        }

        private static decimal GetTotalRain(List<PrecipitationModel> precipData)
        {
            var totalRain = precipData
                .Where(p => p.WeatherType == "rain")
                .Sum(p => p.AmountInches);
            return Math.Round(totalRain,1);
        }

        private static decimal GetTotalSnow(List<PrecipitationModel> precipData)
        {
            var totalSnow = precipData
                 .Where(p => p.WeatherType == "snow")
                 .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }

        private async Task<List<PrecipitationModel>> fetchPrecipitationData(HttpClient httpClient, string zip, int? days)
        {
            var endpoint = BuildPrecipitationServiceEndpoint(zip, days);
            var precipitationRecords = await httpClient.GetAsync(endpoint);
            var precipitationData = await precipitationRecords
                .Content
                .ReadFromJsonAsync<List<PrecipitationModel>>();

            return precipitationData ?? new List<PrecipitationModel>();
        }
        private string BuildPrecipitationServiceEndpoint(string zip, int? days)
        {
            var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherDataConfig.PrecipDataHost;
            var precipServicePort = _weatherDataConfig.PrecipDataPort;
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";

        }

        private async Task<List<TemperatureModel>> fetchTemperatureData(HttpClient httpClient, string zip, int? days)
        {
            var endpoint = BuildTemperatureServiceEndpoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endpoint);
            var temperatureData = await temperatureRecords
                .Content
                .ReadFromJsonAsync<List<TemperatureModel>>();

            return temperatureData ?? new List<TemperatureModel>();
        }

        private string BuildTemperatureServiceEndpoint(string zip, int? days)
        {
            var temperatureServicePorotcol = _weatherDataConfig.TempDataProtocol;
            var temperatureServiceHost = _weatherDataConfig.TempDataHost;
            var temperatureServicePort = _weatherDataConfig.TempDataPort;
            return $"{temperatureServicePorotcol}://{temperatureServiceHost}:{temperatureServicePort}/observation/{zip}?days={days}";

        }
    }
}
