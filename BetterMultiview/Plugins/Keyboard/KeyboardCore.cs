using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KeyboardHooks;

namespace ObsMultiview.Plugins.Keyboard {

    /// <summary>
    /// Parameters of a Keyboard event
    /// </summary>
    public class KeyboardEventArgs {
        public bool IsDown { get; }
        public bool Cancel { get; set; }

        public string Keyboard { get; }

        public int VirtualKey { get; }

        public KeyboardEventArgs(string keyboard, int virtualKey, bool isDown) {
            IsDown = isDown;
            Keyboard = keyboard;
            VirtualKey = virtualKey;
        }
    }

    /// <summary>
    /// Bundles Hook processing and keyboard mappings
    /// </summary>
    internal sealed class KeyboardCore {
        private static Dictionary<string, string> _keyboardMap = new();
        private static int _currentDriverId = -1;

        private readonly DriverHook _driver;
        private readonly RawInputHook _rawInput;
        private readonly LowLevelHook _lowLevel;
        private KeyboardCoreSettings _settings;
        private readonly CommandFacade _management;

        /// <summary>
        /// Whether this instance is currently capturing key events
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Fired on a key event
        /// </summary>
        public event Action<KeyboardEventArgs> KeyEvent;

        public KeyboardCore(CommandFacade management) {
            _driver = new DriverHook();
            _rawInput = new RawInputHook();
            _lowLevel = new LowLevelHook();

            _management = management;
            _management.SettingsChanged += s => {
                if (s == "core") {
                    _settings = management.RequestSettings<KeyboardCoreSettings>("core");
                }
            };
            _settings = management.RequestSettings<KeyboardCoreSettings>("core");

            // only use raw input to map driver keyboard numbers to device id
            _rawInput.KeyEvent += evt => {
                if (_currentDriverId >= 0) {
                    _keyboardMap[_currentDriverId.ToString()] = evt.Keyboard;
                }
            };

            _lowLevel.KeyEvent += evt => {
                if (_currentDriverId < 0) {
                    evt.Intercept = OnKeyEvent(null, evt.KeyCode, evt.IsDown);
                }
            };

            _driver.KeyEvent += evt => {
                if (_currentDriverId < 0) {
                    var keyboard = _keyboardMap.ContainsKey(evt.Keyboard) ? _keyboardMap[evt.Keyboard] : "<unknown>";

                    evt.Intercept = OnKeyEvent(keyboard, evt.KeyCode, evt.IsDown);
                }
            };
        }

        /// <summary>
        /// Enable key capturing
        /// </summary>
        /// <param name="useDriver">Whether to try using the driver for capture</param>
        /// <returns></returns>
        public bool Enable(bool useDriver) {
            if (IsEnabled) return false;

            if (useDriver) {
                var driver = _driver.Hook();
                if (driver) {
                    _rawInput.Hook();
                    VerifyMapping();
                }

                IsEnabled = driver;
                return driver;
            } else {
                var hook = _lowLevel.Hook();
                IsEnabled = hook;
                return hook;
            }
        }

        /// <summary>
        /// Update the driver keyboard mapping
        /// </summary>
        /// <param name="force">Force reevaluation of the mapping</param>
        private void VerifyMapping(bool force = false) {
            if (_driver.IsEnabled) {
                if (_keyboardMap.Count == 0 || force) {
                    _keyboardMap.Clear();

                    Task.Run(() => {
                        ushort key = 0x54;

                        for (int i = 0; i < 10; i++) {
                            _currentDriverId = i;
                            _driver.InjectKey(i, key, true);
                            Thread.Sleep(30);
                        }

                        _currentDriverId = -1;

                        var kbds = _keyboardMap.Where(x => x.Value.StartsWith(@"\\?\")).Select(x => new {
                            deviceId = x.Value,
                            Id = x.Key,
                            usbId = x.Value.Split('#')[1],
                            usbPort = x.Value.Split('#')[2]
                        }).ToList();

                        foreach (var kbd in kbds) {
                            if (!_settings.KeyboardGuid.ContainsKey(kbd.deviceId)) {
                                // search for lookalike
                                var count = kbds.Count(x => x.usbId == kbd.usbId && x.deviceId != kbd.deviceId);
                                if (count > 1 || count == 0) {
                                    // too many devices with same ID
                                    _settings.KeyboardGuid.Add(kbd.deviceId, Guid.NewGuid().ToString());
                                } else if (count == 1) {
                                    var kbdSrc = kbds.First(x => x.usbId == kbd.usbId);
                                    _settings.KeyboardGuid.Add(kbd.deviceId, _settings.KeyboardGuid[kbdSrc.deviceId]);
                                    _settings.KeyboardGuid.Remove(kbdSrc.deviceId);
                                }
                            }
                        }

                        _management.WriteSettings(_settings, "core");
                    });
                }
            }
        }

        /// <summary>
        /// Disable key capturing
        /// </summary>
        public void Disable() {
            _driver.Unhook();
            _lowLevel.Unhook();
            _rawInput.Unhook();
            IsEnabled = false;
        }

        private bool OnKeyEvent(string keyboard, int virtualKey, bool isDown) {
            if (keyboard != null && _settings.KeyboardGuid.ContainsKey(keyboard)) {
                keyboard = _settings.KeyboardGuid[keyboard];
            }

            var args = new KeyboardEventArgs(keyboard, virtualKey, isDown);

            KeyEvent?.Invoke(args);

            return args.Cancel;
        }
    }
}