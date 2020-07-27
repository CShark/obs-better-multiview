using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using StreamDeck.Services.Youtube;

namespace StreamDeck.Converter {
    public class YtStatusConverter:IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is YoutubeStatus ys) {
                switch (ys) {
                    case YoutubeStatus.Ok:
                        return "Verbindung OK";
                    case YoutubeStatus.Bad:
                        return "Schlechte Verbingung";
                    case YoutubeStatus.Good:
                        return "Gute Verbindung";
                    case YoutubeStatus.NoData:
                        return "Keine Verbindung";
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
