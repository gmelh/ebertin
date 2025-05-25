// Add these using statements at the top of your MainWindowViewModel.cs file:
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Ebertin.Views;
using Ebertin.Services;
using Ebertin.Models;
using System.Collections.ObjectModel;


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
        private bool _isNetworkOnline = false; // Default to offline
        
        
        private string _apiLocation = "https://api.ebertinmethod.com";
        private string _api_key = "";
        private string _api_user = "";
        private string _api_email = "";
        private string _api_google_key = "";
        private string _apiKeyDisplay = "Your API key will appear here";
        private bool _isSystemMessageVisible;
        private string _systemMessageText;
        private bool _isProcessing;
        private bool _isUpdatingSuggestions = false;
        private SystemMessageType _systemMessageType;
        
        // Birth Date and Time fields for TextBox controls
        private string _name;
        private string _birthDateText;
        private string _birthTimeText;
        
        // Location Autocomplete Properties
        private string _locationSearchText;
        private bool _isSuggestionsVisible;
        private ObservableCollection<string> _locationSuggestions = new ObservableCollection<string>();
        private string _selectedLocationSuggestion;
        private readonly LocationService _locationService;
        private readonly ChartDataService _chartDataService;
        
        // Initialize the configuration manager and API service
        private readonly ConfigurationManager _configManager = new ConfigurationManager();
        private readonly ApiService _apiService;
        
        // Public properties
        public bool IsFlyoutOpen
        {
            get => _isFlyoutOpen;
            set
            {
                _isFlyoutOpen = value;
                OnPropertyChanged(nameof(IsFlyoutOpen));
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
            get => _api_user;
            set => SetProperty(ref _api_user, value);
        }
        
        public string Email
        {
            get => _api_email;
            set => SetProperty(ref _api_email, value);
        }
        
        public string ApiKeyDisplay
        {
            get => _apiKeyDisplay;
            set => SetProperty(ref _apiKeyDisplay, value);
        }
        
        public string GoogleMapsApiKey
        {
            get => _api_google_key;
            set => SetProperty(ref _api_google_key, value);
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
        
        public bool IsNetworkOnline
        {
            get => _isNetworkOnline;
            set => SetProperty(ref _isNetworkOnline, value);
        }
        
        // Birth date and time text properties (for TextBox controls)
        public string BirthDateText
        {
            get => _birthDateText;
            set => SetProperty(ref _birthDateText, value);
        }
        
        public string BirthTimeText
        {
            get => _birthTimeText;
            set => SetProperty(ref _birthTimeText, value);
        }
        
        public string Name
        {
            get => _name;
            set 
            { 
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Location autocomplete properties
        public string LocationSearchText
        {
            get => _locationSearchText;
            set
            {
                if (SetProperty(ref _locationSearchText, value))
                {
                    // Update suggestions when text changes
                    UpdateLocationSuggestions();
                }
            }
        }

        public bool IsSuggestionsVisible
        {
            get => _isSuggestionsVisible;
            set => SetProperty(ref _isSuggestionsVisible, value);
        }

        public ObservableCollection<string> LocationSuggestions
        {
            get => _locationSuggestions;
            set => SetProperty(ref _locationSuggestions, value);
        }

        public string SelectedLocationSuggestion
        {
            get => _selectedLocationSuggestion;
            set
            {
                if (SetProperty(ref _selectedLocationSuggestion, value) && value != null)
                {
                    // First hide the suggestions
                    IsSuggestionsVisible = false;
            
                    // Then update the text
                    LocationSearchText = value;
            
                    // Clear suggestions to prevent them from showing if popup gets reopened
                    LocationSuggestions.Clear();
                }
            }
        }
        
        public SystemMessageType SystemMessageTypeValue
        {
            get => _systemMessageType;
            set => SetProperty(ref _systemMessageType, value);
        }
        
        public LogOnModalViewModel LogOnModal { get; }

        // Commands
        public ICommand ExitCommand { get; }
        public ICommand Button1Command { get; }
        public ICommand Button2Command { get; }
        public ICommand Button3Command { get; }
        public ICommand NetworkButtonCommand { get; private set; }
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
        public ICommand SelectLocationCommand { get; }
        public ICommand SubmitChartDataCommand { get; }
        
        public ICommand OpenLogOnModalCommand { get; }

        public MainWindowViewModel()
        {
            // Initialize API and location services
            _apiService = new ApiService(_configManager);
            _locationService = new LocationService(_configManager);
            
            // Load configuration values
            _apiLocation = _configManager.ApiLocation;
            _api_user = _configManager.ApiUser;
            _api_email = _configManager.ApiEmail;
            _api_key = _configManager.ApiKey;
            _api_google_key = _configManager.ApiGoogleKey;
            
            if (!string.IsNullOrEmpty(_api_key))
            {
                _apiKeyDisplay = _api_key;
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
            OpenApiKeyWindowCommand = new RelayCommand(OnOpenApiKeyWindow);
            CloseApiKeyModalCommand = new RelayCommand(CloseApiKeyModal);
            CreateApiKeyCommand = new AsyncRelayCommand(CreateApiKeyAsync, () => !IsProcessing);
            RetrieveApiKeyCommand = new AsyncRelayCommand(RetrieveApiKeyAsync, () => !IsProcessing);
            OpenConfigurationModalCommand = new RelayCommand(OpenConfigurationWindow);
            CloseConfigurationModalCommand = new RelayCommand(CloseConfigurationWindow);
            SaveConfigurationCommand = new RelayCommand(SaveConfiguration);
            SelectLocationCommand = new RelayCommand<string>(SelectLocation);
            SubmitChartDataCommand = new RelayCommand(async () => await SubmitChartDataAsync());
            NetworkButtonCommand = new RelayCommand(OnNetworkButtonClick);
            LogOnModal = new LogOnModalViewModel(_configManager, _apiService, SetNetworkStatus);
            OpenLogOnModalCommand = new RelayCommand(OpenLogOnModal);
            
            
            // Initialize location and date/time properties
            _locationSearchText = string.Empty;
            _isSuggestionsVisible = false;
            _birthDateText = string.Empty;
            _birthTimeText = string.Empty;
            
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
        
        private void OnNetworkButtonClick()
        {
            // If network is currently off, show the logon modal instead of just toggling
            if (!IsNetworkOnline)
            {
                Console.WriteLine("Network is off - showing LogOn modal");
                LogOnModal.Open();
            }
            else
            {
                // If network is on, turn it off
                IsNetworkOnline = false;
                Console.WriteLine("Network turned off");
            }
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
            FlyoutWidth = IsFlyoutOpen ? 425 : 0; // Changed from 300 to 425
            Console.WriteLine($"Flyout toggled: {IsFlyoutOpen}, Width: {FlyoutWidth}");
        }

        private void CloseFlyout()
        {
            // Close the flyout
            IsFlyoutOpen = false;
            FlyoutWidth = 0;
            Console.WriteLine("Flyout closed");
            
            // Also hide any suggestion popups
            IsSuggestionsVisible = false;
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
        
        private async void OnOpenApiKeyWindow()
        {
            // Simply show the modal overlay
            IsApiKeyModalVisible = true;
            Console.WriteLine("API Key modal opened");
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
            if (string.IsNullOrWhiteSpace(ApiLocation))
            {
                ShowSystemMessage("Please enter a valid API endpoint", SystemMessageType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                ShowSystemMessage("Please enter a username", SystemMessageType.Error);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowSystemMessage("Please enter an email address", SystemMessageType.Error);
                return;
            }
            
            try
            {
                // Show processing state
                IsProcessing = true;
                ShowSystemMessage("Registering with API...", SystemMessageType.Success);

                
                // Register for an API key
                string apiKey = await _apiService.RegisterApiKeyAsync(ApiLocation,Username, Email);
                
                // Update display
                ApiKeyDisplay = apiKey;
                
                // Show success message
                ShowSystemMessage("API Key created successfully!", SystemMessageType.Success);
                
                Console.WriteLine("API key registration successful");
            }
            catch (Exception ex)
            {
                // Show error message
                ShowSystemMessage($"Error: {ex.Message}", SystemMessageType.Error);
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
                ShowSystemMessage("Please enter an email address", SystemMessageType.Error);
                return;
            }
    
            try
            {
                // Show processing state
                IsProcessing = true;
                ShowSystemMessage("Retrieving API key...", SystemMessageType.Success);
        
                // Call the API to retrieve the key
                string apiKey = await _apiService.GetApiKeyAsync(Email);
        
                // Update the display
                ApiKeyDisplay = apiKey;
        
                // Show success message
                ShowSystemMessage("API Key retrieved successfully.", SystemMessageType.Success);
        
                Console.WriteLine("API Key retrieved from server");
            }
            catch (Exception ex)
            {
                // Get the innermost exception message for more details
                string detailedMessage = GetDetailedExceptionMessage(ex);
        
                // Show error message with more details
                ShowSystemMessage($"Error retrieving API key: {detailedMessage}", SystemMessageType.Error);
                Console.WriteLine($"Error retrieving API key: {detailedMessage}");
        
                // Try to get key from local config as fallback
                string localApiKey = _configManager.ApiKey;
                if (!string.IsNullOrEmpty(localApiKey))
                {
                    ApiKeyDisplay = localApiKey;
                    ShowSystemMessage("Using locally stored API key instead.", SystemMessageType.Warning);                }
            }
            finally
            {
                // Reset processing state
                IsProcessing = false;
            }
        }
        
        private async Task SubmitChartDataAsync()
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrWhiteSpace(Name))
                {
                    // Show error message
                    await Messages.ShowErrorMessage("Please enter a name.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(BirthDateText))
                {
                    await Messages.ShowErrorMessage("Please enter a birth date.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(BirthTimeText))
                {
                    await Messages.ShowErrorMessage("Please enter a birth time.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LocationSearchText))
                {
                    await Messages.ShowErrorMessage("Please enter a birth location.");
                    return;
                }

                // Call the service to submit the data
                await _chartDataService.SubmitChartDataAsync(
                    Name,
                    BirthDateText,
                    BirthTimeText,
                    LocationSearchText
                );

                // Show success message
                await Messages.ShowSuccessMessage("Chart data submitted successfully!");
        
                // Optionally clear the form
                Name = string.Empty;
                BirthDateText = string.Empty;
                BirthTimeText = string.Empty;
                LocationSearchText = string.Empty;
        
                // Optionally close the flyout
                CloseFlyout();
            }
            catch (Exception ex)
            {
                await Messages.ShowErrorMessage($"Failed to submit chart data: {ex.Message}");
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
                ShowSystemMessage("Configuration saved successfully.", SystemMessageType.Success);
                
                Console.WriteLine($"Configuration saved: API_LOCATION={ApiLocation}");
                
                // Close the modal
                IsConfigurationModalVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
                // Show error message
                ShowSystemMessage($"Error saving configuration: {ex.Message}", SystemMessageType.Error);
            }
        }
        
        // ShowSystemMessage method
        private void ShowSystemMessage(string message, SystemMessageType messageType)
        {
            SystemMessageText = message;
            SystemMessageTypeValue = messageType;
            IsSystemMessageVisible = true;
    
            Console.WriteLine($"System message ({messageType}): {message}");
        }
        
        // Method to select a location from the suggestions
        public void SelectLocation(string location)
        {
            if (location != null)
            {
                LocationSearchText = location;
                IsSuggestionsVisible = false;
                Console.WriteLine($"Selected location: {location}");
            }
        }
        
        private void OpenLogOnModal()
        {
            LogOnModal.Open();
        }
        
        private void SetNetworkStatus(bool isOnline)
        {
            IsNetworkOnline = isOnline;
            Console.WriteLine($"Network status set to: {(isOnline ? "Online" : "Offline")}");
        }
        
        // Method to update location suggestions
        private async void UpdateLocationSuggestions()
        {
            // Avoid concurrent updates
            if (_isUpdatingSuggestions)
                return;
        
            _isUpdatingSuggestions = true;
    
            try
            {
                // Don't show suggestions if the text is empty or too short
                if (string.IsNullOrWhiteSpace(LocationSearchText) || LocationSearchText.Length < 2)
                {
                    IsSuggestionsVisible = false;
                    LocationSuggestions.Clear();
                    return;
                }
        
                // Simple check to avoid showing suggestions for already selected text
                foreach (var suggestion in LocationSuggestions)
                {
                    if (LocationSearchText.Equals(suggestion, StringComparison.OrdinalIgnoreCase))
                    {
                        IsSuggestionsVisible = false;
                        return;
                    }
                }
        
                // Get suggestions from the service
                var suggestions = await _locationService.GetLocationSuggestionsAsync(LocationSearchText);
        
                // Update the suggestions collection
                LocationSuggestions.Clear();
                foreach (var suggestion in suggestions)
                {
                    LocationSuggestions.Add(suggestion);
                }
        
                // Show suggestions if there are any
                IsSuggestionsVisible = LocationSuggestions.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching location suggestions: {ex.Message}");
                IsSuggestionsVisible = false;
                LocationSuggestions.Clear();
            }
            finally
            {
                _isUpdatingSuggestions = false;
            }
        }
    }
}