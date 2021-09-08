using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
using Newtonsoft.Json.Linq;

namespace StreamDeck.Plugins.Keyboard {
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
            _core = new KeyboardCore(PluginManagement);

            _core.KeyEvent += evt => {
                if (_slots == null) return;
                if (!evt.IsDown) return;

                bool hit = false;

                if (evt.VirtualKey == _settings.SwitchKey) {
                    if (!_settings.MultipleKeyboardSupport || evt.Keyboard == _settings.SwitchKeyboard) {
                        hit = true;
                        PluginManagement.SwitchLive();
                    }
                }

                if (!hit && (evt.VirtualKey & 0x60) == 0x60) {
                    var num = evt.VirtualKey & 0x0F;
                    if (num >= 0 && num <= 9) {
                        if ((DateTime.Now - _lastEntry).TotalMilliseconds > 500 || _numpadEntry.Length > 3) {
                            _numpadEntry = "";
                        }
                        _numpadEntry += num.ToString();

                        var numpadSlots = _slots.Where(x => x.config.NumpadMode);
                        var slot = numpadSlots.FirstOrDefault(x => x.config.NumpadNumber == Convert.ToInt32(_numpadEntry));

                        if (slot.config != null) {
                            hit = true;
                            PluginManagement.ActivateScene(slot.slot);
                        }
                    }
                }

                if (!hit) {
                    var slots = _slots.Where(x => !x.config.NumpadMode);
                    slots = slots.Where(x => x.config.ShortcutKey == evt.VirtualKey);

                    if (_settings.MultipleKeyboardSupport) {
                        slots = slots.Where(x => x.config.KeyboardId == evt.Keyboard);
                    }

                    var slot = slots.FirstOrDefault(x => true);

                    if (slot.config != null) {
                        hit = true;
                        PluginManagement.ActivateScene(slot.slot);
                    }
                }

                _lastEntry = DateTime.Now;
                evt.Cancel = _settings.InterceptKeystrokes && hit;
            };

            PluginManagement.SettingsChanged += s => {
                _settings = PluginManagement.RequestSettings<KeyboardSettings>();
            };

            PluginManagement.SlotConfigChanged += guid => {
                _slots = PluginManagement.RequestSlotSettings<KeyboardSlotSettings>().ToList();
            };
        }

        public override void OnEnabled() {
            State = PluginState.Active;
            _slots = PluginManagement.RequestSlotSettings<KeyboardSlotSettings>().ToList();
            _settings = PluginManagement.RequestSettings<KeyboardSettings>();
            _core.Enable(_settings.MultipleKeyboardSupport);
        }

        public override void OnDisabled() {
            State = PluginState.Disabled;
            _core.Disable();
        }

        public override void PausePlugin(bool pause) {
            if (State == PluginState.Active) {
                if (pause) {
                    _core.Disable();
                } else {
                    _core.Enable(_settings.MultipleKeyboardSupport);
                }
            }
        }

        public override SettingsControl GetGlobalSettings() {
            return new GlobalSettings(PluginManagement);
        }

        public override SlotSettingsControl GetSlotSettings(Guid id) {
            return new SlotSettings(PluginManagement, id);
        }
    }
}