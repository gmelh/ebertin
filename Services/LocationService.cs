using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks; // Add this namespace import
using System;

namespace Ebertin.Services
{
    public class LocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // Google Maps API key
        
        // Cache for suggestions to reduce API calls
        private Dictionary<string, List<string>> _suggestionCache = new Dictionary<string, List<string>>();
        
        public LocationService(string apiKey = null)
        {
            _httpClient = new HttpClient();
            _apiKey = "AIzaSyAfsio4k3J8zZvkgG0ZPTrY-NlTHpZaJc4"; // Replace with your actual API key
        }
        
        public async Task<List<string>> GetLocationSuggestionsAsync(string searchText)
        {
            // Check cache first
            string cacheKey = searchText.ToLower();
            if (_suggestionCache.ContainsKey(cacheKey))
            {
                return _suggestionCache[cacheKey];
            }
            
            try
            {
                // Use Google Places Autocomplete API
                string url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={Uri.EscapeDataString(searchText)}&types=(cities)&key={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GooglePlacesAutocompleteResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                // Extract the place descriptions
                var suggestions = result.Predictions
                    .Select(p => p.Description)
                    .ToList();
                
                // Cache the results
                _suggestionCache[cacheKey] = suggestions;
                
                return suggestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in location service: {ex.Message}");
                
                // Fallback to offline/dummy data if API call fails
                return GetFallbackSuggestions(searchText);
            }
        }
        
        // Classes to deserialize Google Places API response
        private class GooglePlacesAutocompleteResponse
        {
            public PlacePrediction[] Predictions { get; set; } = Array.Empty<PlacePrediction>();
            public string Status { get; set; }
        }
        
        private class PlacePrediction
        {
            public string Description { get; set; }
            public string PlaceId { get; set; }
        }
        
        // Same fallback method as before
        private List<string> GetFallbackSuggestions(string searchText)
        {
            // Fallback city list implementation remains the same
            var cities = new List<string>
            {
                "New York, United States",
                "London, United Kingdom",
                // ... other cities
            };
            
            return cities
                .Where(city => city.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();
        }
    }
}