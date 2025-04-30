using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Avalonia.IDE.ToolKit.Converters;

public class BrushToColorConverter : IValueConverter
{
    public static readonly BrushToColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
            return brush.Color;
        return Colors.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(color);
        return Brushes.Transparent;
    }
}