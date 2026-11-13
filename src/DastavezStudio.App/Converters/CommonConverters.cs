using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using DastavezStudio.Core.Models;

namespace DastavezStudio.App.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : false;
    }
}

public class StatusColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is BatchItemStatus status)
        {
            return status switch
            {
                BatchItemStatus.Success => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65)),    // Fluent Green
                BatchItemStatus.Failed => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 52, 56)),     // Fluent Red
                BatchItemStatus.Processing => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212)), // Fluent Blue
                BatchItemStatus.Skipped => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 234, 112, 0)),   // Fluent Orange
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128))                         // Gray
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
