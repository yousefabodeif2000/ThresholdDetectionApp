using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ThresholdDetection.WPF.Converters
{
    public class HeatColorConverter : IValueConverter
    {
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
