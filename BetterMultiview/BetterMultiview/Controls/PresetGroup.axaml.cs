using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System;
using System.ComponentModel;
using Avalonia.Interactivity;
using BetterMultiview.Data;
using BetterMultiview.Dialogs;
using BetterMultiview.Data.Presets;

namespace BetterMultiview.Controls
{
    public partial class PresetGroup : UserControl {
        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<PresetGroup, object>(nameof(Header));

        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<PresetGroup, int>(nameof(Rows), 1);

        public static readonly StyledProperty<int> PresetCountProperty =
            AvaloniaProperty.Register<PresetGroup, int>(nameof(PresetCount), 0);

        public static readonly StyledProperty<string> PresetTypeProperty =
            AvaloniaProperty.Register<PresetGroup, string>(nameof(PresetType));

        public static readonly StyledProperty<bool> SingleModeProperty =
            AvaloniaProperty.Register<PresetGroup, bool>(nameof(SingleMode));

        public ObservableCollection<PresetSlot> Presets { get; set; } = new();

        public object Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public int Rows {
            get => GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public int PresetCount {
            get => GetValue(PresetCountProperty);
            set => SetValue(PresetCountProperty, value);
        }

        public string PresetType {
            get => GetValue(PresetTypeProperty);
            set => SetValue(PresetTypeProperty, value);
        }

        public bool SingleMode {
            get => GetValue(SingleModeProperty);
            set => SetValue(SingleModeProperty, value);
        }

        public PresetGroup() {
            InitializeComponent();
            this.GetObservable(PresetCountProperty).Subscribe(value => {
                if (value > Presets.Count) {
                    while (Presets.Count < value) {
                        var slot = new PresetSlot(Presets.Count);
                        slot.PropertyChanged += SlotOnPropertyChanged;
                        Presets.Add(slot);
                    }
                } else if (value < Presets.Count) {
                    while (Presets.Count > value) {
                        var slot = Presets[^1];
                        slot.PropertyChanged -= SlotOnPropertyChanged;
                        Presets.Remove(slot);
                    }
                }
            });
        }

        private void SlotOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (sender is PresetSlot slot && e.PropertyName == nameof(PresetSlot.ButtonState)) {
                if (SingleMode && slot.ButtonState != PresetButtonState.Off) {
                    foreach (var pslot in Presets) {
                        if(pslot == slot) continue;

                        pslot.ButtonState = PresetButtonState.Off;
                    }
                }
            }
        }

        private async void AssignEdit_OnClick(object? sender, RoutedEventArgs e) {
            if (PresetFactory.PresetRegistered(PresetType)) {
                if (sender is Control { DataContext: PresetSlot preset }) {
                    PresetBase clone;
                    clone = preset.Preset?.Clone() ?? PresetFactory.CreateInstance(PresetType);

                    var editor = clone.CreateEditor();
                    var dlg = new Dialog();
                    if (await dlg.Show(Multiview.Instance, "Edit slot", editor) == true) {
                        preset.IsOnAir = false;
                        preset.ButtonState = PresetButtonState.Off;
                        preset.Preset = clone;
                    }
                }
            }
        }

        private void Clear_OnClick(object? sender, RoutedEventArgs e) {
            if (sender is Control { DataContext: PresetSlot preset }) {
                preset.IsOnAir = false;
                preset.ButtonState = PresetButtonState.Off;
                preset.Preset = null;
            }
        }
    }
}