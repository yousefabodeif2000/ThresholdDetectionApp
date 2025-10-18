using System;
using System.Globalization;
using System.Windows.Data;

namespace ThresholdDetection.WPF.Converters
{
    /// <summary>
    /// Converts numeric indices or values to scaled coordinates by multiplying with <see cref="CellSize"/>.
    /// </summary>
    /// <remarks>
    /// This is useful for converting grid indices or logical coordinates into pixel positions
    /// in a WPF UI. Supports both <c>int</c> and <c>double</c> input values.
    /// </remarks>
    [ValueConversion(typeof(int), typeof(double))]
    public class ScaleConverter : IValueConverter
    {
        /// <summary>
        /// The scaling factor applied to input values.
        /// Defaults to 20.0.
        /// </summary>
        public double CellSize { get; set; } = 20.0;

        /// <summary>
        /// Converts an <c>int</c> or <c>double</c> value to a scaled <c>double</c> by multiplying with <see cref="CellSize"/>.
        /// </summary>
        /// <param name="value">The input value, expected to be <c>int</c> or <c>double</c>.</param>
        /// <param name="targetType">The target type (expected to be <c>double</c>).</param>
        /// <param name="parameter">Optional parameter (unused).</param>
        /// <param name="culture">Culture information (unused).</param>
        /// <returns>
        /// The input value multiplied by <see cref="CellSize"/> as a <c>double</c>.
        /// Returns 0 for unsupported input types.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int idx) return idx * CellSize;
            if (value is double d) return d * CellSize;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
