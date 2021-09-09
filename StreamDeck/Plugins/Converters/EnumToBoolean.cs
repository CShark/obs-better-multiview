using System;
using System.Windows.Data;

namespace StreamDeck.Plugins.Converters {
    /// <summary>
    /// convert an enum value to boolean
    /// </summary>
    public class EnumToBoolean : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}