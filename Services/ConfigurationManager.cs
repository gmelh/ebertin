using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ebertin.Services
{
    /// <summary>
    /// Manages configuration settings for the Ebertin application.
    /// </summary>
    public class ConfigurationManager
    {
        private const string CONFIG_FILE_PATH = "/Configuration/Ebertin.cfg";
        private const string API_LOCATION_KEY = "API_LOCATION";
        private const string API_KEY_KEY = "API_KEY";
        
        private readonly string _configFilePath;
        private Dictionary<string, string> _configValues;
        
        /// <summary>
        /// Initializes a new instance of the ConfigurationManager.
        /// </summary>
        public ConfigurationManager()
        {
            // Get the application's base directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _configFilePath = Path.Combine(baseDirectory, CONFIG_FILE_PATH.TrimStart('/'));
            _configValues = new Dictionary<string, string>();
            
            // Ensure the configuration directory exists
            EnsureConfigDirectoryExists();
            
            // Load config values
            LoadConfiguration();
        }
        
        /// <summary>
        /// Gets or sets the configured API location.
        /// </summary>
        public string ApiLocation
        {
            get => GetConfigValue(API_LOCATION_KEY, "https://api.ebertinmethod.com");
            set => SetConfigValue(API_LOCATION_KEY, value);
        }
        
        /// <summary>
        /// Gets or sets the configured API key.
        /// </summary>
        public string ApiKey
        {
            get => GetConfigValue(API_KEY_KEY, string.Empty);
            set => SetConfigValue(API_KEY_KEY, value);
        }
        
        /// <summary>
        /// Loads the configuration from the file.
        /// </summary>
        private void LoadConfiguration()
        {
            _configValues.Clear();
            
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string[] lines = File.ReadAllLines(_configFilePath);
                    
                    foreach (string line in lines)
                    {
                        // Skip empty lines or comments
                        string trimmedLine = line.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                            continue;
                            
                        // Parse key-value pairs
                        int separatorIndex = trimmedLine.IndexOf('=');
                        if (separatorIndex > 0)
                        {
                            string key = trimmedLine.Substring(0, separatorIndex).Trim();
                            string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                            
                            // Remove quotes if present
                            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
                                value = value.Substring(1, value.Length - 2);
                                
                            _configValues[key] = value;
                        }
                    }
                    
                    Console.WriteLine($"Configuration loaded from: {_configFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                // Initialize with default values on error
                _configValues.Clear();
            }
        }
        
        /// <summary>
        /// Saves the configuration to the file.
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                // Build the configuration content
                List<string> lines = new List<string>();
                lines.Add("# Ebertin Configuration File");
                lines.Add($"# Last Updated: {DateTime.Now}");
                lines.Add(string.Empty);
                
                // Add configuration values
                foreach (var kvp in _configValues)
                {
                    // Add quotes for values with spaces
                    string value = kvp.Value.Contains(" ") ? $"\"{kvp.Value}\"" : kvp.Value;
                    lines.Add($"{kvp.Key}={value}");
                }
                
                // Write to file
                File.WriteAllLines(_configFilePath, lines);
                Console.WriteLine($"Configuration saved to: {_configFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                throw; // Re-throw to allow caller to handle
            }
        }
        
        /// <summary>
        /// Ensures the configuration directory exists.
        /// </summary>
        private void EnsureConfigDirectoryExists()
        {
            string directoryPath = Path.GetDirectoryName(_configFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Created configuration directory: {directoryPath}");
            }
        }
        
        /// <summary>
        /// Gets a configuration value with a default fallback.
        /// </summary>
        private string GetConfigValue(string key, string defaultValue)
        {
            return _configValues.TryGetValue(key, out string value) ? value : defaultValue;
        }
        
        /// <summary>
        /// Sets a configuration value.
        /// </summary>
        private void SetConfigValue(string key, string value)
        {
            _configValues[key] = value;
        }
        
        /// <summary>
        /// Asynchronously saves the configuration.
        /// </summary>
        public async Task SaveConfigurationAsync()
        {
            await Task.Run(() => SaveConfiguration());
        }
    }
}