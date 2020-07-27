using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StreamDeck.Services.Stream;

namespace StreamDeck.Converter {
    public class StatusConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is StreamState ss) {
                switch (ss) {
                    case StreamState.Offline:
                        return "Offline";
                    case StreamState.Live:
                        return "Live";
                    case StreamState.Preparing:
                        return "Bereit";
                    case StreamState.Completed:
                        return "Abgeschlossen";
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}