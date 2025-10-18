using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ThresholdDetection.WPF.Converters
{
    /// <summary>
    /// Converts a numeric value in the range [0,1] to a heatmap-style color.
    /// </summary>
    /// <remarks>
    /// Values are clamped between 0 and 1:
    /// <list type="bullet">
    /// <item>0 = Blue (<c>RGB(0,0,255)</c>)</item>
    /// <item>1 = Red (<c>RGB(255,0,0)</c>)</item>
    /// <item>Intermediate values = interpolated between blue and red</item>
    /// </list>
    /// Returns <see cref="Brushes.Transparent"/> if the input value is not a <c>double</c>.
    /// </remarks>
    public class HeatColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <c>double</c> value to a <see cref="SolidColorBrush"/> representing a heatmap color.
        /// </summary>
        /// <param name="value">The input value, expected to be a <c>double</c> between 0 and 1.</param>
        /// <param name="targetType">The target type (expected to be <see cref="Brush"/>).</param>
        /// <param name="parameter">Optional parameter (unused).</param>
        /// <param name="culture">Culture information (unused).</param>
        /// <returns>
        /// A <see cref="SolidColorBrush"/> with color interpolated between blue (0) and red (1),
        /// or <see cref="Brushes.Transparent"/> if the input is not a <c>double</c>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                d = Math.Clamp(d, 0, 1);
                byte r = (byte)(255 * d);
                byte b = (byte)(255 * (1 - d));
                return new SolidColorBrush(Color.FromRgb(r, 0, b));
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
