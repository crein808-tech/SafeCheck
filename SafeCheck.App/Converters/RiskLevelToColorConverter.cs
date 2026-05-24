using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SafeCheck.Models;

namespace SafeCheck.Converters;

public class RiskLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not RiskLevel level) return Brushes.Gray;
        return level switch
        {
            RiskLevel.Safe => new SolidColorBrush(Color.FromRgb(16, 124, 16)),
            RiskLevel.Caution => new SolidColorBrush(Color.FromRgb(255, 185, 0)),
            RiskLevel.Warning => new SolidColorBrush(Color.FromRgb(255, 140, 0)),
            RiskLevel.Danger => new SolidColorBrush(Color.FromRgb(218, 59, 1)),
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RiskLevelToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not RiskLevel level) return "CHECKING...";
        return level switch
        {
            RiskLevel.Safe => "LIKELY SAFE",
            RiskLevel.Caution => "USE CAUTION",
            RiskLevel.Warning => "WARNING",
            RiskLevel.Danger => "HIGH RISK",
            _ => "UNKNOWN"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RiskLevelToEmojiConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not RiskLevel level) return "?";
        return level switch
        {
            RiskLevel.Safe => "✅",
            RiskLevel.Caution => "⚠️",
            RiskLevel.Warning => "⚠️",
            RiskLevel.Danger => "\U0001F6A8",
            _ => "❓"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
