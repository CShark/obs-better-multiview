using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StreamDeck.Converters {

    /// <summary>
    /// Convert a string to visibility (null or whitespace => hidden)
    /// </summary>
    public class StringToVis : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string s) {
                return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
            } else if (value == null) {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}