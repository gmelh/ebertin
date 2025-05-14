// ChartDataService.cs
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Ebertin.Services
{
    public interface IChartDataService
    {
        Task<ChartDataResponse> SubmitChartDataAsync(string name, string birthDate, string birthTime, string location);
        Task<ChartResult> GetChartResultAsync(string chartId);
        Task<bool> ValidateLocationAsync(string location);
    }

    public class ChartDataService : IChartDataService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiEndpoint;
        private readonly string _apiKey;

        public ChartDataService(HttpClient httpClient, string apiEndpoint, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
            _apiKey = apiKey;
        }

        public async Task<ChartDataResponse> SubmitChartDataAsync(string name, string birthDate, string birthTime, string location)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(birthDate))
                throw new ArgumentException("Birth date is required.", nameof(birthDate));
            if (string.IsNullOrWhiteSpace(birthTime))
                throw new ArgumentException("Birth time is required.", nameof(birthTime));
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location is required.", nameof(location));

            // Create the request payload
            var requestData = new ChartDataRequest
            {
                Name = name,
                BirthDate = birthDate,
                BirthTime = birthTime,
                Location = location,
                Timestamp = DateTime.UtcNow,
                TimeZone = GetTimeZoneFromLocation(location) // Stub method
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add API key to headers if provided
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            }

            try
            {
                // Make the API call
                var response = await _httpClient.PostAsync($"{_apiEndpoint}/api/v1/charts", content);
                
                // Ensure success
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var responseBody = await response.Content.ReadAsStringAsync();
                var chartResponse = JsonSerializer.Deserialize<ChartDataResponse>(responseBody);
                
                return chartResponse;
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP errors
                throw new ChartDataServiceException($"Failed to submit chart data to API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors
                throw new ChartDataServiceException($"Failed to parse API response: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Handle other errors
                throw new ChartDataServiceException($"Unexpected error submitting chart data: {ex.Message}", ex);
            }
        }

        public async Task<ChartResult> GetChartResultAsync(string chartId)
        {
            if (string.IsNullOrWhiteSpace(chartId))
                throw new ArgumentException("Chart ID is required.", nameof(chartId));

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/api/v1/charts/{chartId}");
                response.EnsureSuccessStatusCode();
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var chartResult = JsonSerializer.Deserialize<ChartResult>(responseBody);
                
                return chartResult;
            }
            catch (HttpRequestException ex)
            {
                throw new ChartDataServiceException($"Failed to retrieve chart data: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateLocationAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return false;

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/api/v1/locations/validate?q={Uri.EscapeDataString(location)}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private string GetTimeZoneFromLocation(string location)
        {
            // Stub implementation - would normally use a geolocation service
            // For now, return a default timezone
            return "UTC";
        }
    }

    // Request/Response models
    public class ChartDataRequest
    {
        public string Name { get; set; }
        public string BirthDate { get; set; }
        public string BirthTime { get; set; }
        public string Location { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeZone { get; set; }
    }

    public class ChartDataResponse
    {
        public string ChartId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProcessingUrl { get; set; }
    }

    public class ChartResult
    {
        public string ChartId { get; set; }
        public string Name { get; set; }
        public DateTime BirthDateTime { get; set; }
        public LocationInfo Location { get; set; }
        public ChartData Data { get; set; }
        public string Status { get; set; }
    }

    public class LocationInfo
    {
        public string City { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string TimeZone { get; set; }
    }

    public class ChartData
    {
        public object Planets { get; set; } // Would contain planetary positions
        public object Houses { get; set; }  // Would contain house data
        public object Aspects { get; set; } // Would contain aspect data
        // Add more properties as needed
    }

    // Custom exception for service errors
    public class ChartDataServiceException : Exception
    {
        public ChartDataServiceException(string message) : base(message) { }
        public ChartDataServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}