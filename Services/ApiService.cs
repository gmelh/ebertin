using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ebertin.Services
{
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
                // Get API URL from configuration
                string apiUrl = _configManager.ApiLocation;

                // Ensure the API URL is available
                if (string.IsNullOrWhiteSpace(apiUrl))
                    throw new InvalidOperationException("API URL is not configured");

                // Create the get-key endpoint URL
                string getKeyUrl = $"{apiUrl.TrimEnd('/')}/get-key";

                // Create the request payload as JSON
                var requestData = new
                {
                    email = email
                };

                // Serialize to JSON
                string jsonContent = JsonSerializer.Serialize(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send the POST request with JSON content
                Console.WriteLine($"Sending get-key request to: {getKeyUrl}");
                var response = await _httpClient.PostAsync(getKeyUrl, httpContent);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response received: {responseContent}");
                
                // Try to extract the API key using the flexible method
                string apiKey = ExtractApiKey(responseContent);
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception($"Could not extract API key from response: {responseContent}");
                }
                
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
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON error during API key retrieval: {ex.Message}");
                throw new Exception(
                    "Failed to parse the API response. The server may have returned an unexpected format.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during API key retrieval: {ex.Message}");
                throw new Exception("An unexpected error occurred during API key retrieval.", ex);
            }
        }
        
        /// <summary>
        /// Attempts to extract an API key from various possible response formats.
        /// </summary>
        /// <param name="responseContent">The JSON response content.</param>
        /// <returns>The extracted API key, or null if not found.</returns>
        private string ExtractApiKey(string responseContent)
        {
            // First try standard JSON parsing
            try
            {
                using (JsonDocument document = JsonDocument.Parse(responseContent))
                {
                    // Try common property names for API key
                    string[] possiblePropertyNames = new[] { "apiKey", "api_key", "key", "token", "api_token" };
                    
                    foreach (string propName in possiblePropertyNames)
                    {
                        if (document.RootElement.TryGetProperty(propName, out JsonElement keyElement))
                        {
                            return keyElement.GetString();
                        }
                    }
                    
                    // If not found in properties, check if the response itself is just the key as a string
                    if (document.RootElement.ValueKind == JsonValueKind.String)
                    {
                        return document.RootElement.GetString();
                    }
                    
                    // Check if the response is an array with the first element being the key
                    if (document.RootElement.ValueKind == JsonValueKind.Array && 
                        document.RootElement.GetArrayLength() > 0 && 
                        document.RootElement[0].ValueKind == JsonValueKind.String)
                    {
                        return document.RootElement[0].GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, check if the response is just a plain text key
                string cleaned = responseContent.Trim('"', ' ', '\t', '\r', '\n');
                if (!string.IsNullOrWhiteSpace(cleaned) && cleaned.Length > 8) // Assuming keys are at least 8 chars
                {
                    return cleaned;
                }
            }
            
            // Could not extract a key
            return null;
        }
        
        /// <summary>
        /// Debug helper method to examine the structure of an API response.
        /// </summary>
        /// <param name="responseContent">The JSON response content.</param>
        /// <returns>A description of the response structure.</returns>
        private string DescribeResponseStructure(string responseContent)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(responseContent))
                {
                    var root = document.RootElement;
                    StringBuilder description = new StringBuilder();
                    
                    // Describe the root element type
                    description.AppendLine($"Response is a {root.ValueKind}");
                    
                    // If it's an object, list all properties
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        description.AppendLine("Properties:");
                        foreach (JsonProperty prop in root.EnumerateObject())
                        {
                            description.AppendLine($"  - {prop.Name}: {prop.Value.ValueKind}");
                        }
                    }
                    // If it's an array, describe the first few elements
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        int count = root.GetArrayLength();
                        description.AppendLine($"Array with {count} elements:");
                        
                        int displayCount = Math.Min(count, 5);
                        for (int i = 0; i < displayCount; i++)
                        {
                            description.AppendLine($"  - Element {i}: {root[i].ValueKind}");
                        }
                    }
                    // If it's a simple value, show it
                    else if (root.ValueKind == JsonValueKind.String)
                    {
                        description.AppendLine($"String value: {root.GetString()}");
                    }
                    else if (root.ValueKind == JsonValueKind.Number)
                    {
                        description.AppendLine($"Number value: {root.GetDouble()}");
                    }
                    else if (root.ValueKind == JsonValueKind.True || root.ValueKind == JsonValueKind.False)
                    {
                        description.AppendLine($"Boolean value: {root.GetBoolean()}");
                    }
                    
                    return description.ToString();
                }
            }
            catch (JsonException ex)
            {
                return $"Not a valid JSON: {ex.Message}\nRaw content: {responseContent}";
            }
        }
    }
}