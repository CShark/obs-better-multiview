using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using OBS.WebSocket.NET.Types;

namespace StreamDeck.Converter {
    class StreamColorConverter : IValueConverter {
        public Brush Started { get; set; }
        public Brush Starting { get; set; }
        public Brush Stopping { get; set; }
        public Brush Stopped { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is OutputState state) {
                return state switch {
                    OutputState.Started => Started,
                    OutputState.Starting => Starting,
                    OutputState.Stopped => Stopped,
                    OutputState.Stopping => Stopping,
                    _ => value
                };
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
