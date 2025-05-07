using System;
using Avalonia.Layout;

namespace DesignerLibrary;

/// <summary>
/// Вспомогательный класс для работы с перечислениями в XAML.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Возвращает все значения перечисления <see cref="HorizontalAlignment"/>.
    /// </summary>
    public static HorizontalAlignment[]? HorizontalAlignments => Enum.GetValues(typeof(HorizontalAlignment)) as HorizontalAlignment[];
    
    /// <summary>
    /// Возвращает все значения перечисления <see cref="HorizontalAlignment"/>.
    /// </summary>
    public static VerticalAlignment[]? VerticalAlignments => Enum.GetValues(typeof(VerticalAlignment)) as VerticalAlignment[];
}