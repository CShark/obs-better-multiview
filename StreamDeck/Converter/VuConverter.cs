using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StreamDeck.Converter {
    class VuConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length == 2) {
                if(values[0] is float f && values[1] is double height) {
                    if (float.IsInfinity(f)) {
                        return height;
                    }

                    f = (f + 64) / 64f;

                    return (1 - f) * height;
                }
            }

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}