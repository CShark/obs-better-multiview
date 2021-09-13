using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ObsMultiview.Plugins.KNX {
    public class DatapointTemplateSelector : Grid {
        public static readonly DependencyProperty DPT1Property = DependencyProperty.Register(
            nameof(DPT1), typeof(UIElement), typeof(DatapointTemplateSelector),
            new PropertyMetadata(default(UIElement)));

        public UIElement DPT1 {
            get { return (UIElement) GetValue(DPT1Property); }
            set { SetValue(DPT1Property, value); }
        }

        public static readonly DependencyProperty DPT5Property = DependencyProperty.Register(
            nameof(DPT5), typeof(UIElement), typeof(DatapointTemplateSelector),
            new PropertyMetadata(default(UIElement)));

        public UIElement DPT5 {
            get { return (UIElement) GetValue(DPT5Property); }
            set { SetValue(DPT5Property, value); }
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(KnxDatapointType), typeof(DatapointTemplateSelector),
            new PropertyMetadata(default(KnxDatapointType),
                (o, args) => ((DatapointTemplateSelector) o).UpdateControl()));

        public KnxDatapointType Type {
            get { return (KnxDatapointType) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public DatapointTemplateSelector() {
            Initialized += (sender, args) => { UpdateControl(); };
        }

        private void UpdateControl() {
            Children.Clear();

            switch (Type) {
                case KnxDatapointType.Dpt1:
                    Children.Add(DPT1);
                    break;
                case KnxDatapointType.Dpt5:
                    Children.Add(DPT5);
                    break;
            }
        }
    }

    public class DatapointConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is KnxDatapointType type) {
                try {
                    if (value is byte[] data) {
                        switch (type) {
                            case KnxDatapointType.Dpt1:
                                return data.Aggregate(0, (x, i) => x + i) != 0;
                            case KnxDatapointType.Dpt5:
                                return data.Length > 0 ? (int)(data[0] / 2.55f) : 0;
                            default:
                                return value;
                        }
                    } else {
                        switch (type) {
                            case KnxDatapointType.Dpt5:
                                return 0;
                            default:
                                return value;
                        }
                    }
                } catch (Exception ex) {
                    return value;
                }
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter is KnxDatapointType type) {
                try {
                    switch (type) {
                        case KnxDatapointType.Dpt1:
                            var dpt1 = System.Convert.ToBoolean(value);
                            return dpt1 ? new byte[] {0x01} : new byte[] {0x00};
                        case KnxDatapointType.Dpt5:
                            if (value is string s) {
                                value = s.Replace("%", "");
                            }

                            var dpt5 = (byte) (System.Convert.ToInt32(value) * 2.55f);
                            return new[] {dpt5};
                        default:
                            return value;
                    }
                } catch (Exception ex) {
                    return value;
                }
            } else {
                return value;
            }
        }
    }

    public class NullConverter : IValueConverter {
        public object Null { get; set; }
        public object NotNull { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is null ? Null : NotNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}