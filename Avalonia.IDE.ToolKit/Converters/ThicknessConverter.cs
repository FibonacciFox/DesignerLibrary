using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Avalonia.IDE.ToolKit.Converters;

/// <summary>
/// Конвертер, который преобразует значение int в равномерный Thickness.
/// Используется для привязки размеров якорей и внутренних отступов.
/// </summary>
public class ThicknessConverter : IValueConverter
{
    public static readonly ThicknessConverter Instance = new ThicknessConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int size)
        {
            return new Thickness(size);
        }
        
        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Thickness thickness &&
            thickness.Left == thickness.Top &&
            thickness.Left == thickness.Right &&
            thickness.Left == thickness.Bottom)
        {
            return (int)thickness.Left;
        }

        return AvaloniaProperty.UnsetValue;
    }
}