using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Windows.Documents;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamDeck.Extensions;
using WPFLocalizeExtension.Engine;

namespace StreamDeck.Plugins.Keyboard {
    /// <summary>
    /// Plugin to handle keyboard shortcuts
    /// </summary>
    public class KeyboardPlugin : PluginBase {
        private KeyboardCore _core;
        private KeyboardSettings _settings;
        private List<(Guid slot, KeyboardSlotSettings config)> _slots;
        private string _numpadEntry = "";
        private DateTime _lastEntry = DateTime.MinValue;

        public override string Name => "Keyboard Shortcuts";
        public override string Author => "Nathanael Schneider";
        public override string Version => "1.0";

        public override bool HasSettings => true;
        public override bool HasSlotSettings => true;

        public KeyboardPlugin() {
        }

        protected override void Initialize() {
            Logger.LogInformation("Initializing Plugin");
            _core = new KeyboardCore(CommandFacade);

            _core.KeyEvent += evt => {
                if (_slots == null) return;
                if (!evt.IsDown) return;

                bool hit = false;

                // first catch if we are switching to live
                if (evt.VirtualKey == _settings.SwitchKey) {
                    if (!_settings.MultipleKeyboardSupport || evt.Keyboard == _settings.SwitchKeyboard) {
                        hit = true;
                        CommandFacade.SwitchLive();
                    }
                }

                // then catch numbers
                if (!hit && (evt.VirtualKey & 0x60) == 0x60) {
                    var num = evt.VirtualKey & 0x0F;
                    if (num >= 0 && num <= 9) {
                        if ((DateTime.Now - _lastEntry).TotalMilliseconds > 500 || _numpadEntry.Length > 3) {
                            _numpadEntry = "";
                        }

                        _numpadEntry += num.ToString();

                        var numpadSlots = _slots.Where(x => x.config.NumpadMode);
                        var slot = numpadSlots.FirstOrDefault(x =>
                            x.config.NumpadNumber == Convert.ToInt32(_numpadEntry));

                        if (slot.config != null) {
                            hit = true;
                            CommandFacade.ActivateScene(slot.slot);
                        }
                    }
                }

                // at last, catch direct shortcut keys
                if (!hit) {
                    var slots = _slots.Where(x => !x.config.NumpadMode);
                    slots = slots.Where(x => x.config.ShortcutKey == evt.VirtualKey);

                    if (_settings.MultipleKeyboardSupport) {
                        slots = slots.Where(x => x.config.KeyboardId == evt.Keyboard);
                    }

                    var slot = slots.FirstOrDefault(x => true);

                    if (slot.config != null) {
                        hit = true;
                        CommandFacade.ActivateScene(slot.slot);
                    }
                }

                _lastEntry = DateTime.Now;
                evt.Cancel = _settings.InterceptKeystrokes && hit;
            };

            CommandFacade.SettingsChanged += s => { _settings = CommandFacade.RequestSettings<KeyboardSettings>(); };

            CommandFacade.SlotConfigChanged += guid => {
                _slots = CommandFacade.RequestSlotSettings<KeyboardSlotSettings>().ToList();
            };
        }

        public override void OnEnabled() {
            _settings = CommandFacade.RequestSettings<KeyboardSettings>();
            Logger.LogInformation($"Enabling plugin, DriverMode={_settings.MultipleKeyboardSupport}");
            State = PluginState.Active;
            InfoMessage = "";
            _slots = CommandFacade.RequestSlotSettings<KeyboardSlotSettings>().ToList();

            if (!_core.Enable(_settings.MultipleKeyboardSupport)) {
                Logger.LogError("Failed to enable keyboard hook");
                State = PluginState.Faulted;
                InfoMessage = Localizer.Localize<string>("Keyboard", "Hook.Failed") +
                              (_settings.MultipleKeyboardSupport
                                  ? Localizer.Localize<string>("Keyboard", "Hook.FailedDriver")
                                  : Localizer.Localize<string>("Keyboard", "Hook.FailedHook"));
            }
        }

        public override void OnDisabled() {
            Logger.LogInformation("Disabling plugin");
            State = PluginState.Disabled;
            InfoMessage = "";
            _core.Disable();
        }

        public override void PausePlugin(bool pause) {
            // Pause keyboard capture during an active plugin settings dialog
            if (State == PluginState.Active) {
                if (pause) {
                    _core.Disable();
                } else {
                    _core.Enable(_settings.MultipleKeyboardSupport);
                }
            }
        }

        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(CommandFacade);
        }

        public override SettingsControl GetSlotSettings(Guid id) {
            return new SlotSettings(CommandFacade, id);
        }
    }
}