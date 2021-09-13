using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ObsMultiview.Converters {
    /// <summary>
    /// Convert a boolean to a color
    /// </summary>
    public class BoolToColorConverter : IValueConverter {
        public Brush Default { get; set; }
        public Brush True { get; set; }
        public Brush False { get; set; }

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
}