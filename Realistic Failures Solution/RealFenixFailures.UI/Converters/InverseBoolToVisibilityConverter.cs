using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RealFenixFailures.UI.Converters;

/// <summary>
/// Convierte un bool a Visibility invertido: false → Visible, true → Collapsed.
/// Útil para estados vacíos y elementos mutuamente excluyentes.
/// </summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        var flag = value is bool b && b;
        return flag ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is Visibility v && v != Visibility.Visible;
    }
}
