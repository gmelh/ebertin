using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Ebertin.Models;

namespace Ebertin.ViewModels
{
    public abstract class BaseModalViewModel : INotifyPropertyChanged
    {
        private bool _isVisible;
        private bool _isSystemMessageVisible;
        private string _systemMessageText = string.Empty;
        private SystemMessageType _systemMessageType;
        private bool _isProcessing;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Common properties for all modals
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
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

        public SystemMessageType SystemMessageTypeValue
        {
            get => _systemMessageType;
            set => SetProperty(ref _systemMessageType, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        // Common commands
        public ICommand CloseCommand { get; }

        protected BaseModalViewModel()
        {
            CloseCommand = new RelayCommand(OnClose);
        }

        // Virtual methods that can be overridden
        protected virtual void OnClose()
        {
            IsVisible = false;
            IsSystemMessageVisible = false;
            Console.WriteLine($"{GetType().Name} modal closed");
        }

        // Helper method for showing system messages
        protected void ShowSystemMessage(string message, SystemMessageType messageType)
        {
            SystemMessageText = message;
            SystemMessageTypeValue = messageType;
            IsSystemMessageVisible = true;

            Console.WriteLine($"System message ({messageType}): {message}");
        }

        // Virtual method for opening the modal (can be overridden for initialization)
        public virtual void Open()
        {
            IsVisible = true;
            IsSystemMessageVisible = false;
            Console.WriteLine($"{GetType().Name} modal opened");
        }
    }
}