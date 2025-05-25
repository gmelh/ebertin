using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Ebertin.Models;
using Ebertin.Services;

namespace Ebertin.ViewModels
{
    public class LogOnModalViewModel : BaseModalViewModel
    {
        private string _apiLocation = "https://api.ebertinmethod.com";
        private string _email = string.Empty;
        private string _password = string.Empty;
        
        private readonly ConfigurationManager _configManager;
        private readonly ApiService _apiService;
        private readonly Action<bool> _setNetworkStatus; // Callback to set network status

        public string ApiLocation
        {
            get => _apiLocation;
            set => SetProperty(ref _apiLocation, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // Commands specific to LogOn
        public ICommand LogOnCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public LogOnModalViewModel(ConfigurationManager configManager, ApiService apiService, Action<bool> setNetworkStatus)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _setNetworkStatus = setNetworkStatus ?? throw new ArgumentNullException(nameof(setNetworkStatus));

            // Initialize commands
            LogOnCommand = new AsyncRelayCommand(LogOnAsync, () => !IsProcessing);
            SaveSettingsCommand = new RelayCommand(SaveSettings);

            // Load saved settings
            LoadSettings();
        }

        private void LoadSettings()
        {
            ApiLocation = _configManager.ApiLocation;
            // Don't load email/password for security reasons - user must enter them each time
        }

        private void SaveSettings()
        {
            try
            {
                _configManager.ApiLocation = ApiLocation;
                _configManager.SaveConfiguration();

                ShowSystemMessage("Settings saved successfully.", SystemMessageType.Success);
                Console.WriteLine($"LogOn settings saved: API_LOCATION={ApiLocation}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving LogOn settings: {ex.Message}");
                ShowSystemMessage($"Error saving settings: {ex.Message}", SystemMessageType.Error);
            }
        }

        private async Task LogOnAsync()
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(ApiLocation))
            {
                ShowSystemMessage("Please enter a valid API endpoint", SystemMessageType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowSystemMessage("Please enter an email address", SystemMessageType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowSystemMessage("Please enter a password", SystemMessageType.Error);
                return;
            }

            try
            {
                IsProcessing = true;
                ShowSystemMessage("Logging in...", SystemMessageType.Success);

                // Call your authentication service here
                // bool success = await _apiService.LogInAsync(ApiLocation, Email, Password);
                
                // For now, simulate the login
                await Task.Delay(1000); // Simulate network call
                
                // Replace this with actual login logic
                bool loginSuccess = true; // This should come from your API call
                
                if (loginSuccess)
                {
                    ShowSystemMessage("Login successful!", SystemMessageType.Success);
                    
                    // Set network status to online via callback
                    _setNetworkStatus(true);
                    
                    // Close modal after successful login
                    await Task.Delay(1500); // Show success message briefly
                    OnClose();
                }
                else
                {
                    ShowSystemMessage("Login failed. Please check your credentials.", SystemMessageType.Error);
                }
            }
            catch (Exception ex)
            {
                ShowSystemMessage($"Error during login: {ex.Message}", SystemMessageType.Error);
                Console.WriteLine($"Login error: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected override void OnClose()
        {
            // Clear sensitive data when closing
            Password = string.Empty;
            base.OnClose();
        }

        public override void Open()
        {
            // Load fresh settings when opening
            LoadSettings();
            base.Open();
        }
    }
}