using System;
using System.Globalization;
using System.Windows.Data;

namespace ObsMultiview.Plugins.Converters
{
    /// <summary>
    /// Convert a boolean to arbitrary values
    /// </summary>
    class BoolConverter:IValueConverter
    {
        public object True { get; set; }
        public object False { get; set; }
        public object Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b) {
                return b ? True : False;
            }

            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    class BoolInvert:IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b) {
                return !b;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
