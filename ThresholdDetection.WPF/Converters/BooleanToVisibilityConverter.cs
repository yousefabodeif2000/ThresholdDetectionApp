using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThresholdDetection.WPF.Converters
{
    /// <summary>
    /// Converts a boolean value to a <see cref="Visibility"/> value for WPF UI elements.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="parameter"/> is the string "invert" (case-insensitive),
    /// the conversion is inverted: <c>true</c> becomes <see cref="Visibility.Collapsed"/>
    /// and <c>false</c> becomes <see cref="Visibility.Visible"/>.
    /// </remarks>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to a <see cref="Visibility"/> value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type (expected to be <see cref="Visibility"/>).</param>
        /// <param name="parameter">
        /// Optional parameter. If set to "invert" (case-insensitive), the conversion result is inverted.
        /// </param>
        /// <param name="culture">The culture information (unused).</param>
        /// <returns>
        /// <see cref="Visibility.Visible"/> if <paramref name="value"/> is <c>true</c> 
        /// (or <c>false</c> if inverted), otherwise <see cref="Visibility.Collapsed"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            bool visible = (bool)value;
            if (invert) visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
