using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using System;

namespace BetterMultiview.Controls {
    [PseudoClasses(":preview", ":onair", ":empty", ":toggle", ":previewOff")]
    public class PresetButton : Button {
        public static readonly StyledProperty<PresetButtonState> ButtonStateProperty =
            AvaloniaProperty.Register<PresetButton, PresetButtonState>(nameof(ButtonState));

        public PresetButtonState ButtonState {
            get => GetValue(ButtonStateProperty);
            set => SetValue(ButtonStateProperty, value);
        }

        public static readonly StyledProperty<bool> CanToggleProperty =
            AvaloniaProperty.Register<PresetButton, bool>(nameof(CanToggle));

        public bool CanToggle {
            get => GetValue(CanToggleProperty);
            set => SetValue(CanToggleProperty, value);
        }

        public static readonly StyledProperty<bool> IsOnAirProperty =
            AvaloniaProperty.Register<PresetButton, bool>(nameof(IsOnAir));

        public bool IsOnAir {
            get => GetValue(IsOnAirProperty);
            set => SetValue(IsOnAirProperty, value);
        }

        public static readonly StyledProperty<bool> IsUnboundProperty =
            AvaloniaProperty.Register<PresetButton, bool>(nameof(IsUnbound));

        public bool IsUnbound {
            get => GetValue(IsUnboundProperty);
            set => SetValue(IsUnboundProperty, value);
        }

        protected override void OnClick() {
            if (IsUnbound) return;

            if (!CanToggle) {
                if (ButtonState == PresetButtonState.Off) {
                    ButtonState = PresetButtonState.Preview;
                } else {
                    ButtonState = PresetButtonState.Off;
                }
            } else {
                ButtonState = ButtonState switch {
                    PresetButtonState.Off => PresetButtonState.Preview,
                    PresetButtonState.Preview => PresetButtonState.Toggle,
                    PresetButtonState.Toggle => PresetButtonState.PreviewOff,
                    PresetButtonState.PreviewOff => PresetButtonState.Off,
                    _ => PresetButtonState.Off
                };
            }

            base.OnClick();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            this.GetObservable(IsUnboundProperty).Subscribe(val => {
                PseudoClasses.Set(":empty", val);
                if (val == true) {
                    IsOnAir = false;
                    ButtonState = PresetButtonState.Off;
                }
            });

            this.GetObservable(IsOnAirProperty).Subscribe(val => { PseudoClasses.Set(":onair", val); });

            this.GetObservable(ButtonStateProperty).Subscribe(val => {
                PseudoClasses.Set(":preview", false);
                PseudoClasses.Set(":toggle", false);
                PseudoClasses.Set(":previewOff", false);

                switch (ButtonState) {
                    case PresetButtonState.Preview:
                        PseudoClasses.Set(":preview", true);
                        break;
                    case PresetButtonState.Toggle:
                        PseudoClasses.Set(":toggle", true);
                        break;
                    case PresetButtonState.PreviewOff:
                        PseudoClasses.Set(":previewOff", true);
                        break;
                }    
            });

            base.OnApplyTemplate(e);
        }
    }

    public enum PresetButtonState {
        Off,
        Preview,
        Toggle,
        PreviewOff
    }
}