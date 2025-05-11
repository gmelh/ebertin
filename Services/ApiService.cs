using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Ebertin.Models.HttpClientExtensions;

namespace Ebertin.Services
{
    // Add this inside the Ebertin.Services namespace temporarily
    public class ApiKeyResponse
    {
        public string api_key { get; set; }
    }
    
    /// <summary>
    /// Service for interacting with the Ebertin API.
    /// </summary>
    public class ApiService
    {
        private readonly ConfigurationManager _configManager;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the ApiService.
        /// </summary>
        /// <param name="configManager">The configuration manager instance.</param>
        public ApiService(ConfigurationManager configManager)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Registers a new user with the API and obtains an API key.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <param name="email">The user's email address.</param>
        /// <returns>A task representing the registration process with the API key as the result.</returns>
        public async Task<string> RegisterApiKeyAsync(string api_location, string name, string email)
        {
            _configManager.ApiLocation = api_location;
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name must be provided", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided", nameof(email));

            try
            {
                // Get API URL from configuration
                string apiUrl = _configManager.ApiLocation;

                // Ensure the API URL is available
                if (string.IsNullOrWhiteSpace(apiUrl))
                    throw new InvalidOperationException("API URL is not configured");

                // Create the register endpoint URL
                string registerUrl = $"{apiUrl.TrimEnd('/')}/register";

                // Create the registration payload
                var registrationData = new
                {
                    name = name,
                    email = email
                };

                // Serialize the payload
                string jsonContent = JsonSerializer.Serialize(registrationData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send the registration request
                Console.WriteLine($"Sending registration request to: {registerUrl}");
                var response = await _httpClient.PostAsync(registerUrl, httpContent);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received: {responseContent}");

                // Try to extract the API key using the flexible method
                /*
                string apiKey = ExtractApiKey(responseContent);
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception($"Could not extract API key from response: {responseContent}");
                }
                
                
                // Save the API key to configuration
                _configManager.ApiKey = apiKey;
                */
                await _configManager.SaveConfigurationAsync();
                
                return "status: OK";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error during API registration: {ex.Message}");
                throw new Exception(
                    "Failed to connect to the API server. Please check your API Location and try again.", ex);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON error during API registration: {ex.Message}");
                throw new Exception(
                    "Failed to parse the API response. The server may have returned an unexpected format.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during API registration: {ex.Message}");
                throw new Exception("An unexpected error occurred during API registration.", ex);
            }
        }

        /// <summary>
        /// Retrieves an existing API key using the user's email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>A task representing the retrieval process with the API key as the result.</returns>
        public async Task<string> GetApiKeyAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided", nameof(email));

            try
            {
                string apiUrl = _configManager.ApiLocation;
                if (string.IsNullOrWhiteSpace(apiUrl))
                    throw new InvalidOperationException("API URL is not configured");

                string getKeyUrl = $"{apiUrl.TrimEnd('/')}/get-key";

                var requestData = new { email = email };
                string jsonContent = JsonSerializer.Serialize(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending get-key request to: {getKeyUrl}");
        
                // This is the correct way to call the extension method
                var result = await _httpClient.PostAsJsonAsync<ApiKeyResponse>(getKeyUrl, httpContent);
        
                string apiKey = result.api_key;
                Console.WriteLine($"API Key: {apiKey}");
        
                // Save the API key to configuration
                _configManager.ApiKey = apiKey;
                _configManager.ApiEmail = email;
                await _configManager.SaveConfigurationAsync();
        
                return apiKey;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error during API key retrieval: {ex.Message}");
                throw new Exception(
                    "Failed to connect to the API server. Please check your API Location and try again.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during API key retrieval: {ex.Message}");
                throw new Exception("An unexpected error occurred during API key retrieval.", ex);
            }
        }        
    }
}