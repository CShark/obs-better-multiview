using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace StreamDeck.Converter {
    [ContentProperty(nameof(Steps))]
    class RangeToColor : IValueConverter {
        public List<ColorStep> Steps { get; set; } = new List<ColorStep>();
        public Brush DefaultColor { get; set; } = Brushes.LightGray;

        private List<ColorStep> _sorted;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (_sorted == null) {
                _sorted = Steps.OrderByDescending(x => x.Min).ToList();
            }

            if (value is float f) {
                return _sorted.FirstOrDefault(x=>x.Min <= f)?.Color ?? DefaultColor;
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    class ColorStep {
        public float Min { get; set; }
        public Brush Color { get; set; }
    }
}