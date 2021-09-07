using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using StreamDeck.Plugins.Annotations;

namespace StreamDeck.Plugins {
    public enum PluginState {
        Active,
        Warning,
        Faulted,
        Disabled,
    }

    public abstract class SettingsControl : UserControl {
        public abstract void FetchSettings();
        public abstract void WriteSettings();
    }

    public abstract class SettingsControl<T> : SettingsControl {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(T), typeof(SettingsControl), new PropertyMetadata(default(T)));

        public T Settings {
            get { return (T) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        protected PluginManagement PluginManagement { get; }

        protected SettingsControl(PluginManagement management) {
            PluginManagement = management;
        }

        public sealed override void FetchSettings() {
            Settings = PluginManagement.RequestSettings<T>();
        }

        public sealed override void WriteSettings() {
            PluginManagement.WriteSettings(Settings);
        }
    }

    public abstract class SlotSettingsControl : UserControl {
    }

    public abstract class SlotSettingsControl<T> : SlotSettingsControl {
    }

    public abstract class PluginBase : INotifyPropertyChanged {
        private PluginState _state = PluginState.Disabled;
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual bool HasSettings { get; } = false;

        public virtual bool HasSlotSettings { get; } = false;

        protected PluginManagement PluginManagement { get; private set; }

        public PluginState State {
            get => _state;
            protected set {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public abstract void OnEnabled();

        public abstract void OnDisabled();

        public abstract SettingsControl GetGlobalSettings();

        public abstract SlotSettingsControl GetSlotSettings();

        public void SetPluginManagement(PluginManagement management) {
            if (PluginManagement == null)
                PluginManagement = management;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class PluginManagement {
        private Func<string, JObject> _resolveSettings;
        private Action<string, JObject> _writeSettings;

        public PluginManagement(Func<string, JObject> resolveSettings, Action<string, JObject> writeSettings) {
            _resolveSettings = resolveSettings;
            _writeSettings = writeSettings;
        }

        public T RequestSettings<T>(string subtype = null) {
            var json = _resolveSettings(subtype);

            if (json != null) {
                return json.ToObject<T>();
            } else {
                return new JObject().ToObject<T>();
            }
        }

        public void WriteSettings<T>(T settings, string subtype = null) {
            var json = JObject.FromObject(settings);
            _writeSettings(subtype, json);
            OnSettingsChanged(subtype);
        }

        public event Action<string> SettingsChanged;

        private void OnSettingsChanged(string obj) {
            SettingsChanged?.Invoke(obj);
        }
    }
}