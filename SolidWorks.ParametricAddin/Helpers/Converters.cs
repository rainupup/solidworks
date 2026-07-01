using System;
using System.Windows;
using System.Windows.Data;

namespace SolidWorks.ParametricAddin.Helpers
{
    /// <summary>
    /// Converts a boolean to Visibility.Visible (true) or Visibility.Collapsed (false).
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                bool invert = parameter is string s && s == "Invert";
                return invert
                    ? (boolValue ? Visibility.Collapsed : Visibility.Visible)
                    : (boolValue ? Visibility.Visible : Visibility.Collapsed);
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }
    }

    /// <summary>
    /// Converts the inverse of a boolean to Visibility (true=Collapsed, false=Visible).
    /// Useful for showing/hiding elements based on a negated condition.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility != Visibility.Visible;
            return false;
        }
    }
}
