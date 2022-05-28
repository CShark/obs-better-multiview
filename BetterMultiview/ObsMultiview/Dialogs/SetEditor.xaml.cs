using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ObsMultiview.Data;

namespace ObsMultiview.Dialogs {
    /// <summary>
    /// Interaktionslogik für SetEditor.xaml
    /// </summary>
    public partial class SetEditor : Window {
        public static readonly DependencyProperty SetsProperty = DependencyProperty.Register(
            nameof(Sets), typeof(ObservableCollection<Set>), typeof(SetEditor),
            new PropertyMetadata(default(ObservableCollection<Set>)));

        public ObservableCollection<Set> Sets {
            get { return (ObservableCollection<Set>)GetValue(SetsProperty); }
            set { SetValue(SetsProperty, value); }
        }

        public static RoutedUICommand EditSet { get; }

        static SetEditor() {
            EditSet = new RoutedUICommand();
        }

        public SetEditor() {
            InitializeComponent();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void EditSet_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var set = e.Parameter as Set;

            if (set != null) {
                var editor = new SetConfig(set);
                editor.Owner = this;
                editor.ShowDialog();
            }
        }

        private void EditSet_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            var set = e.Parameter as Set;

            if (set != null) {
                e.CanExecute = set.Id != null && set.Id != Guid.Empty;
            }
        }
    }
}