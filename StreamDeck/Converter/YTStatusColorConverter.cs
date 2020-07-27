using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using StreamDeck.Services.Youtube;

namespace StreamDeck.Converter {
    public class YtStatusColorConverter :IValueConverter{
        public Brush Ok { get; set; }
        public Brush Good { get; set; }
        public Brush Bad { get; set; }
        public Brush NoData { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is YoutubeStatus ss) {
                switch (ss) {
                    case YoutubeStatus.Ok:
                        return Ok;
                    case YoutubeStatus.Good:
                        return Good;
                    case YoutubeStatus.Bad:
                        return Bad;
                    case YoutubeStatus.NoData:
                        return NoData;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
