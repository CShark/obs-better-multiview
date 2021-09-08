using System;
using System.Collections.Generic;
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
        public abstract void FetchSettings();
        public abstract void WriteSettings();
    }

    public abstract class SlotSettingsControl<T> : SlotSettingsControl {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(T), typeof(SlotSettingsControl), new PropertyMetadata(default(T)));

        public T Settings {
            get { return (T) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        protected PluginManagement PluginManagement { get; }
        protected Guid SlotID { get; }

        protected SlotSettingsControl(PluginManagement pluginManagement, Guid slotID) {
            PluginManagement = pluginManagement;
            SlotID = slotID;
        }

        public override void FetchSettings() {
            Settings = PluginManagement.RequestSlotSetting<T>(SlotID);
        }

        public override void WriteSettings() {
            PluginManagement.WriteSlotSettings(SlotID, Settings);
        }
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

        public virtual void PausePlugin(bool pause) {
        }

        public abstract SettingsControl GetGlobalSettings();

        public abstract SlotSettingsControl GetSlotSettings(Guid slot);


        public void SetPluginManagement(PluginManagement management) {
            if (PluginManagement == null) {
                PluginManagement = management;
                Initialize();
            }
        }

        protected virtual void Initialize() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public abstract class PluginManagement {
        public event Action<string> SettingsChanged;
        public event Action<Guid> SlotConfigChanged;

        public T RequestSettings<T>(string subtype = null) {
            var json = RequestSettings(subtype);

            if (json != null) {
                return json.ToObject<T>();
            } else {
                return new JObject().ToObject<T>();
            }
        }

        public void WriteSettings<T>(T settings, string subtype = null) {
            var json = JObject.FromObject(settings);
            WriteSettings(json, subtype);
            OnSettingsChanged(subtype);
        }

        public T RequestSlotSetting<T>(Guid slot) {
            var json = RequestSlotSetting(slot);

            if (json != null) {
                return json.ToObject<T>();
            } else {
                return new JObject().ToObject<T>();
            }
        }

        public IEnumerable<(Guid id, T config)> RequestSlotSettings<T>() {
            foreach (var item in RequestSlotSettings()) {
                if (item.Item2 != null) {
                    yield return (item.Item1, item.Item2.ToObject<T>());
                }
            }
        }

        public void WriteSlotSettings<T>(Guid id, T config) {
            WriteSlotSettings(id, JObject.FromObject(config));
            OnSlotConfigChanged(id);
        }

        public abstract void ActivateScene(Guid scene);
        public abstract void SwitchLive();

        protected abstract JObject RequestSettings(string subtype = null);
        protected abstract void WriteSettings(JObject settings, string subtype = null);
        protected abstract JObject RequestSlotSetting(Guid slot);
        protected abstract IEnumerable<(Guid id, JObject config)> RequestSlotSettings();
        protected abstract void WriteSlotSettings(Guid id, JObject config);

        private void OnSettingsChanged(string obj) {
            SettingsChanged?.Invoke(obj);
        }

        private void OnSlotConfigChanged(Guid obj) {
            SlotConfigChanged?.Invoke(obj);
        }
    }
}