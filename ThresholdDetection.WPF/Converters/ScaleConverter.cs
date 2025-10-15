using System;
using System.Globalization;
using System.Windows.Data;

namespace ThresholdDetection.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(double))]
    public class ScaleConverter : IValueConverter
    {
        public double CellSize { get; set; } = 20.0;

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
