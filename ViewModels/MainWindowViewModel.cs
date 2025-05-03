using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Ebertin.Views;
using Ebertin.Services;

namespace Ebertin.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isFlyoutOpen;
        private double _flyoutWidth;
        private bool _isButton2Active;
        private bool _isButton3Active;
        private bool _isTopButton1Active;
        private bool _isTopButton2Active;
        private bool _isTopButton3Active;
        private UserControl? _currentCanvas;
        private bool _isApiKeyModalVisible;
        private bool _isConfigurationModalVisible;
        
        private string _apiLocation = "https://api.ebertinmethod.com";
        private string _username;
        private string _email;
        private string _apiKeyDisplay = "Your API key will appear here";
        private bool _isSystemMessageVisible;
        private string _systemMessageText;
        private bool _isProcessing;
        
        // Initialize the configuration manager and API service
        private readonly ConfigurationManager _configManager = new ConfigurationManager();
        private readonly ApiService _apiService;
        
        // Public properties
        public bool IsFlyoutOpen
        {
            get => _isFlyoutOpen;
            set
            {
                if (SetProperty(ref _isFlyoutOpen, value))
                {
                    Console.WriteLine($"IsFlyoutOpen changed to: {value}");
                }
            }
        }
        
        public double FlyoutWidth
        {
            get => _flyoutWidth;
            set
            {
                if (SetProperty(ref _flyoutWidth, value))
                {
                    Console.WriteLine($"FlyoutWidth changed to: {value}");
                }
            }
        }
        
        public bool IsButton2Active
        {
            get => _isButton2Active;
            set => SetProperty(ref _isButton2Active, value);
        }
        
        public bool IsButton3Active
        {
            get => _isButton3Active;
            set => SetProperty(ref _isButton3Active, value);
        }
        
        public bool IsTopButton1Active
        {
            get => _isTopButton1Active;
            set => SetProperty(ref _isTopButton1Active, value);
        }
        
        public bool IsTopButton2Active
        {
            get => _isTopButton2Active;
            set => SetProperty(ref _isTopButton2Active, value);
        }
        
        public bool IsTopButton3Active
        {
            get => _isTopButton3Active;
            set => SetProperty(ref _isTopButton3Active, value);
        }
        
        public UserControl? CurrentCanvas
        {
            get => _currentCanvas;
            set => SetProperty(ref _currentCanvas, value);
        }
        
        public bool IsApiKeyModalVisible
        {
            get => _isApiKeyModalVisible;
            set => SetProperty(ref _isApiKeyModalVisible, value);
        }
        
        public bool IsConfigurationModalVisible
        {
            get => _isConfigurationModalVisible;
            set => SetProperty(ref _isConfigurationModalVisible, value);
        }
        
        public string ApiLocation
        {
            get => _apiLocation;
            set => SetProperty(ref _apiLocation, value);
        }
        
        // API Key related properties
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        public string ApiKeyDisplay
        {
            get => _apiKeyDisplay;
            set => SetProperty(ref _apiKeyDisplay, value);
        }
        
        public bool IsSystemMessageVisible
        {
            get => _isSystemMessageVisible;
            set => SetProperty(ref _isSystemMessageVisible, value);
        }
        
        public string SystemMessageText
        {
            get => _systemMessageText;
            set => SetProperty(ref _systemMessageText, value);
        }
        
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        // Commands
        public ICommand ExitCommand { get; }
        public ICommand Button1Command { get; }
        public ICommand Button2Command { get; }
        public ICommand Button3Command { get; }
        public ICommand LeftButton1Command { get; }
        public ICommand LeftButton2Command { get; }
        public ICommand LeftButton3Command { get; }
        public ICommand CloseFlyoutCommand { get; }
        public ICommand OpenApiKeyWindowCommand { get; }
        public ICommand CloseApiKeyModalCommand { get; }
        public ICommand CreateApiKeyCommand { get; }
        public ICommand RetrieveApiKeyCommand { get; }
        public ICommand OpenConfigurationModalCommand { get; }
        public ICommand CloseConfigurationModalCommand { get; }
        public ICommand SaveConfigurationCommand { get; }

        public MainWindowViewModel()
        {
            // Initialize API service
            _apiService = new ApiService(_configManager);
            
            // Load configuration values
            _apiLocation = _configManager.ApiLocation;
            string apiKey = _configManager.ApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                _apiKeyDisplay = apiKey;
            }
            
            // Initialize commands
            ExitCommand = new RelayCommand(Exit);
            Button1Command = new RelayCommand(OnButton1Click);
            Button2Command = new RelayCommand(OnButton2Click);
            Button3Command = new RelayCommand(OnButton3Click);
            LeftButton1Command = new RelayCommand(ToggleFlyout);
            LeftButton2Command = new RelayCommand(OnLeftButton2Click);
            LeftButton3Command = new RelayCommand(OnLeftButton3Click);
            CloseFlyoutCommand = new RelayCommand(CloseFlyout);
            OpenApiKeyWindowCommand = new RelayCommand(OpenApiKeyWindow);
            CloseApiKeyModalCommand = new RelayCommand(CloseApiKeyModal);
            CreateApiKeyCommand = new AsyncRelayCommand(CreateApiKeyAsync, () => !IsProcessing);
            RetrieveApiKeyCommand = new AsyncRelayCommand(RetrieveApiKeyAsync, () => !IsProcessing);
            OpenConfigurationModalCommand = new RelayCommand(OpenConfigurationWindow);
            CloseConfigurationModalCommand = new RelayCommand(CloseConfigurationWindow);
            SaveConfigurationCommand = new RelayCommand(SaveConfiguration);
            
            
            FlyoutWidth = 0; // Initially closed
            IsFlyoutOpen = false;
            IsButton2Active = false;
            IsButton3Active = false;
            IsTopButton1Active = true;  // Start with the first button active
            IsTopButton2Active = false;
            IsTopButton3Active = false;
            IsApiKeyModalVisible = false; // Initially closed
            IsConfigurationModalVisible = false;
            
            // Initialize with SingleDialCanvas
            CurrentCanvas = new SingleDialCanvas();
            
            Console.WriteLine("MainWindowViewModel initialized");
        }

        private void Exit()
        {
            // This will close the application
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private void OnButton1Click()
        {
            // Set button 1 active and others inactive
            SetTopButtonState(1);
            Console.WriteLine("Top Button 1 clicked - Active");
        }

        private void OnButton2Click()
        {
            // Set button 2 active and others inactive
            SetTopButtonState(2);
            Console.WriteLine("Top Button 2 clicked - Active");
        }

        private void OnButton3Click()
        {
            // Set button 3 active and others inactive
            SetTopButtonState(3);
            Console.WriteLine("Top Button 3 clicked - Active");
        }
        
        private void SetTopButtonState(int activeButton)
        {
            IsTopButton1Active = (activeButton == 1);
            IsTopButton2Active = (activeButton == 2);
            IsTopButton3Active = (activeButton == 3);
            
            // Switch canvas based on active button
            CurrentCanvas = activeButton switch
            {
                1 => new SingleDialCanvas(),
                2 => new BiDialCanvas(),
                3 => new TriDialCanvas(),
                _ => new SingleDialCanvas()
            };
        }

        private void ToggleFlyout()
        {
            // Toggle the flyout
            IsFlyoutOpen = !IsFlyoutOpen;
            FlyoutWidth = IsFlyoutOpen ? 300 : 0;
            Console.WriteLine($"Flyout toggled: {IsFlyoutOpen}, Width: {FlyoutWidth}");
        }

        private void CloseFlyout()
        {
            // Close the flyout
            IsFlyoutOpen = false;
            FlyoutWidth = 0;
            Console.WriteLine("Flyout closed");
        }

        private void OnLeftButton2Click()
        {
            // Toggle Button 2 state
            IsButton2Active = !IsButton2Active;
            Console.WriteLine($"Left Button 2 clicked - Active: {IsButton2Active}");
        }

        private void OnLeftButton3Click()
        {
            // Toggle Button 3 state
            IsButton3Active = !IsButton3Active;
            Console.WriteLine($"Left Button 3 clicked - Active: {IsButton3Active}");
        }
        
        private void OpenApiKeyWindow()
        {
            // Reset message and load current API key if available
            IsSystemMessageVisible = false;
            string apiKey = _configManager.ApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                ApiKeyDisplay = apiKey;
            }
            else
            {
                ApiKeyDisplay = "Your API key will appear here";
            }
            
            // Open the Api key modal window
            IsApiKeyModalVisible = true;
            Console.WriteLine("Api Key modal opened");
        }
        
        private void CloseApiKeyModal()
        {
            // Close the Api key modal window
            IsApiKeyModalVisible = false;
            IsSystemMessageVisible = false;
            Console.WriteLine("Api Key modal closed");
        }
        
        /// <summary>
        /// Gets a detailed message by walking the exception chain to the innermost exception.
        /// </summary>
        private string GetDetailedExceptionMessage(Exception ex)
        {
            if (ex == null)
                return "Unknown error";
        
            // Walk down to the innermost exception
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
            }
    
            return currentEx.Message;
        }
        
        private async Task CreateApiKeyAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Username))
            {
                ShowSystemMessage("Please enter a username", false);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowSystemMessage("Please enter an email address", false);
                return;
            }
            
            try
            {
                // Show processing state
                IsProcessing = true;
                ShowSystemMessage("Registering with API...", true);
                
                // Register for an API key
                string apiKey = await _apiService.RegisterApiKeyAsync(Username, Email);
                
                // Update display
                ApiKeyDisplay = apiKey;
                
                // Show success message
                ShowSystemMessage("API Key created successfully!", true);
                
                Console.WriteLine("API key registration successful");
            }
            catch (Exception ex)
            {
                // Show error message
                ShowSystemMessage($"Error: {ex.Message}", false);
                Console.WriteLine($"API key registration failed: {ex.Message}");
            }
            finally
            {
                // Reset processing state
                IsProcessing = false;
            }
        }
        
        private async Task RetrieveApiKeyAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowSystemMessage("Please enter an email address", false);
                return;
            }
    
            try
            {
                // Show processing state
                IsProcessing = true;
                ShowSystemMessage("Retrieving API key...", true);
        
                // Call the API to retrieve the key
                string apiKey = await _apiService.GetApiKeyAsync(Email);
        
                // Update the display
                ApiKeyDisplay = apiKey;
        
                // Show success message
                ShowSystemMessage("API Key retrieved successfully.", true);
        
                Console.WriteLine("API Key retrieved from server");
            }
            catch (Exception ex)
            {
                // Get the innermost exception message for more details
                string detailedMessage = GetDetailedExceptionMessage(ex);
        
                // Show error message with more details
                ShowSystemMessage($"Error retrieving API key: {detailedMessage}", false);
                Console.WriteLine($"Error retrieving API key: {detailedMessage}");
        
                // Try to get key from local config as fallback
                string localApiKey = _configManager.ApiKey;
                if (!string.IsNullOrEmpty(localApiKey))
                {
                    ApiKeyDisplay = localApiKey;
                    ShowSystemMessage("Using locally stored API key instead.", true);
                }
            }
            finally
            {
                // Reset processing state
                IsProcessing = false;
            }
        }
        
        private void OpenConfigurationWindow()
        {
            // Refresh Api location from config in case it changed externally
            ApiLocation = _configManager.ApiLocation;
            OnPropertyChanged(nameof(ApiLocation));
            
            // Open the Configuration modal window
            IsConfigurationModalVisible = true;
            Console.WriteLine("Configuration modal opened");
        }
        
        private void CloseConfigurationWindow()
        {
            // Close the Configuration modal window
            IsConfigurationModalVisible = false;
            Console.WriteLine("Configuration modal closed");
        }
        
        private void SaveConfiguration()
        {
            try
            {
                // Save Api location to the config
                _configManager.ApiLocation = ApiLocation;
                _configManager.SaveConfiguration();
                
                // Show success message
                ShowSystemMessage("Configuration saved successfully.", true);
                
                Console.WriteLine($"Configuration saved: API_LOCATION={ApiLocation}");
                
                // Close the modal
                IsConfigurationModalVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                // Show error message
                ShowSystemMessage($"Error saving configuration: {ex.Message}", false);
            }
        }
        
        private void ShowSystemMessage(string message, bool isSuccess)
        {
            SystemMessageText = message;
            IsSystemMessageVisible = true;
            
            // Assuming you have a way to set the message type (success, error, etc.)
            // SystemMessageType = isSuccess ? MessageType.Success : MessageType.Error;
            
            Console.WriteLine($"System message ({(isSuccess ? "Success" : "Error")}): {message}");
        }
    }
}