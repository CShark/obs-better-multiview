using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ObsMultiview.Plugins.Converters {
    public class CompareToVisibility : IValueConverter {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.Equals(parameter) ^ Invert) {
                return Visibility.Visible;
            } else {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}