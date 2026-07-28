using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AgentView.Wpf;

/// <summary>Collapses the element when the bound string is null or empty.</summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Yields the message's RoleLabel when set, otherwise the role name.</summary>
public sealed class RoleLabelConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is [string { Length: > 0 } label, ..])
            return label;
        return values is [_, MessageRole role] ? role.ToString() : "";
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
