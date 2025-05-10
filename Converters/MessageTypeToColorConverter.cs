using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Ebertin.Models;
using Avalonia.Media;

// Create a converter class
public class MessageTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SystemMessageType messageType && parameter is string colorType)
        {
            switch (messageType)
            {
                case SystemMessageType.Success:
                    return colorType == "Background" ? "#DFF0D8" : "#3C763D";
                case SystemMessageType.Warning:
                    return colorType == "Background" ? "#FCF8E3" : "#8A6D3B";
                case SystemMessageType.Error:
                    return colorType == "Background" ? "#F2DEDE" : "#A94442";
            }
        }
        return "#DFF0D8"; // Default to success color
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}