﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamDeck.Plugins.KNX {
    /// <summary>
    /// Interaktionslogik für DatapointEditor.xaml
    /// </summary>
    public partial class DatapointEditor : UserControl {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(IList<byte>), typeof(DatapointEditor), new PropertyMetadata(default(IList<byte>)));

        public IList<byte> Data {
            get { return (IList<byte>) GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
            nameof(Type), typeof(KnxDatapointType), typeof(DatapointEditor), new PropertyMetadata(default(KnxDatapointType)));

        public KnxDatapointType Type {
            get { return (KnxDatapointType) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public DatapointEditor() {
            InitializeComponent();
        }
    }
}