using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ThreshView.Views.Converters;

// Expects a ImageDocumentViewModel as value and returns a SolidColorBrush representing the overlay color (OverlayR/G/B/A)
public class OverlayColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value == null) return Brushes.Transparent;
        var vm = value as dynamic;
        try
        {
            byte r = vm.OverlayR;
            byte g = vm.OverlayG;
            byte b = vm.OverlayB;
            byte a = vm.OverlayA;
            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}