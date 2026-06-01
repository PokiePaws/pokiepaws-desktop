using System;
using System.Globalization;
using System.Windows.Data;

namespace PokiePawsDesk.Core
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return "";
            return ((double)value).ToString("0.00", CultureInfo.InvariantCulture);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s || string.IsNullOrWhiteSpace(s)) return null;
            s = s.Replace(',', '.');
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return System.Windows.DependencyProperty.UnsetValue;
        }
    }
}