using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace StreamDeck.Plugins {
    /// <summary>
    /// The current state of the plugin
    /// </summary>
    public enum PluginState {
        Active,
        Warning,
        Faulted,
        Disabled,
    }

    /// <summary>
    /// A settings control for plugins
    /// </summary>
    public abstract class SettingsControl : UserControl {
        /// <summary>
        /// Tell the control to fetch its settings
        /// </summary>
        public abstract void FetchSettings();

        /// <summary>
        /// Tell the control to write its settings back
        /// </summary>
        public abstract void WriteSettings();
    }

    /// <summary>
    /// A settings control for the global plugin settings
    /// </summary>
    /// <typeparam name="T">The Global Settings class for this plugin</typeparam>
    public abstract class SettingsControl<T> : SettingsControl {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(T), typeof(SettingsControl), new PropertyMetadata(default(T)));

        /// <summary>
        /// The global settings of this plugin
        /// </summary>
        public T Settings {
            get { return (T) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        /// <summary>
        /// A facade to interface with the normal application
        /// </summary>
        protected CommandFacade CommandFacade { get; }

        protected SettingsControl(CommandFacade management) {
            CommandFacade = management;
        }

        /// <inheritdoc/>
        public sealed override void FetchSettings() {
            Settings = CommandFacade.RequestSettings<T>();
        }

        /// <inheritdoc/>
        public sealed override void WriteSettings() {
            CommandFacade.WriteSettings(Settings);
        }
    }

    /// <summary>
    /// A settings control for the slot based settings
    /// </summary>
    /// <typeparam name="T">The Slot Settings class for this plugin</typeparam>
    public abstract class SlotSettingsControl<T> : SettingsControl {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings), typeof(T), typeof(SettingsControl), new PropertyMetadata(default(T)));

        public T Settings {
            get { return (T) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        /// <summary>
        /// A facade to interface with the normal application
        /// </summary>
        protected CommandFacade CommandFacade { get; }

        /// <summary>
        /// The ID of the current slot
        /// </summary>
        /// <remarks>ID is not persistent through restarts</remarks>
        protected Guid SlotID { get; }

        protected SlotSettingsControl(CommandFacade commandFacade, Guid slotID) {
            CommandFacade = commandFacade;
            SlotID = slotID;
        }

        /// <inheritdoc/>
        public override void FetchSettings() {
            Settings = CommandFacade.RequestSlotSetting<T>(SlotID);
        }

        /// <inheritdoc/>
        public override void WriteSettings() {
            CommandFacade.WriteSlotSettings(SlotID, Settings);
        }
    }

    /// <summary>
    /// Base class for Plugins
    /// </summary>
    public abstract class PluginBase : INotifyPropertyChanged {
        private PluginState _state = PluginState.Disabled;
        private string _infoMessage;
        
        /// <summary>
        /// The logging instance for this plugin
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Name of the Plugin
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Author of the Plugin
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// Version of the Plugin
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Whether this plugin has global settings
        /// </summary>
        public virtual bool HasSettings { get; } = false;

        /// <summary>
        /// Whether this plugin has slot specific settings
        /// </summary>
        public virtual bool HasSlotSettings { get; } = false;

        /// <summary>
        /// A facade to interface with the normal application
        /// </summary>
        protected CommandFacade CommandFacade { get; private set; }

        /// <summary>
        /// Info message to show when hovering over the plugin status
        /// </summary>
        public string InfoMessage {
            get => _infoMessage;
            set {
                if (value == _infoMessage) return;
                _infoMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Current state of the Plugin
        /// </summary>
        public PluginState State {
            get => _state;
            protected set {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets called to enable the Plugin
        /// </summary>
        public abstract void OnEnabled();

        /// <summary>
        /// Gets called to disable the Plugin
        /// </summary>
        public abstract void OnDisabled();

        /// <summary>
        /// Gets called to pause execution of the plugin during a config dialog for this plugin. May be ignored
        /// </summary>
        /// <param name="pause"></param>
        public virtual void PausePlugin(bool pause) {
        }

        /// <summary>
        /// Requests a control for the global settings
        /// </summary>
        /// <returns></returns>
        public abstract SettingsControl GetGlobalSettings();

        /// <summary>
        /// Requests a control for the slot settings
        /// </summary>
        /// <param name="slot">The slot id</param>
        /// <returns></returns>
        public abstract SettingsControl GetSlotSettings(Guid slot);

        /// <summary>
        /// Set the command facade for this plugin. Can only be set once (during initialization)
        /// </summary>
        /// <param name="management"></param>
        public void SetCommandFacade(CommandFacade management, ILogger logger) {
            if (CommandFacade == null) {
                Logger = logger;
                CommandFacade = management;
                Initialize();
            }
        }

        /// <summary>
        /// Gets called after the command facade is available
        /// </summary>
        protected virtual void Initialize() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// A facade to interface with the normal application
    /// </summary>
    public abstract class CommandFacade {
        /// <summary>
        /// Fired when the settings have changed
        /// </summary>
        public event Action<string> SettingsChanged;

        /// <summary>
        /// Fired when a slot config has changed
        /// </summary>
        public event Action<Guid> SlotConfigChanged;

        /// <summary>
        /// The logging instance for this plugin
        /// </summary>
        public ILogger Logger { get; protected set; }

        /// <summary>
        /// Request settings
        /// </summary>
        /// <typeparam name="T">The settings class</typeparam>
        /// <param name="subtype">The subtype to fetch or null for the default settings</param>
        /// <returns></returns>
        public T RequestSettings<T>(string subtype = null) {
            var json = RequestSettings(subtype);

            if (json != null) {
                return json.ToObject<T>();
            } else {
                return new JObject().ToObject<T>();
            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        /// <typeparam name="T">The settings class</typeparam>
        /// <param name="settings">The global settings</param>
        /// <param name="subtype">The subtype of the settings</param>
        public void WriteSettings<T>(T settings, string subtype = null) {
            var json = JObject.FromObject(settings);
            WriteSettings(json, subtype);
            OnSettingsChanged(subtype);
        }

        /// <summary>
        /// Request settings for a slot
        /// </summary>
        /// <typeparam name="T">The settings class</typeparam>
        /// <param name="slot">The slot id</param>
        /// <returns></returns>
        public T RequestSlotSetting<T>(Guid slot) {
            var json = RequestSlotSetting(slot);

            if (json != null) {
                return json.ToObject<T>();
            } else {
                return new JObject().ToObject<T>();
            }
        }

        /// <summary>
        /// Request all slot settings with their IDs
        /// </summary>
        /// <typeparam name="T">The settings class</typeparam>
        /// <returns></returns>
        public IEnumerable<(Guid id, T config)> RequestSlotSettings<T>() {
            foreach (var item in RequestSlotSettings()) {
                if (item.Item2 != null) {
                    yield return (item.Item1, item.Item2.ToObject<T>());
                }
            }
        }

        /// <summary>
        /// Save slot settings
        /// </summary>
        /// <typeparam name="T">The settings class</typeparam>
        /// <param name="id">The slot ID</param>
        /// <param name="config">The slot settings</param>
        public void WriteSlotSettings<T>(Guid id, T config) {
            WriteSlotSettings(id, JObject.FromObject(config));
            OnSlotConfigChanged(id);
        }

        /// <summary>
        /// Activate a scene in preview mode
        /// </summary>
        /// <param name="scene">The scene ID</param>
        public abstract void ActivateScene(Guid scene);

        /// <summary>
        /// Switch the currently live and preview scenes
        /// </summary>
        public abstract void SwitchLive();

        /// <inheritdoc cref="RequestSettings{T}"/>
        protected abstract JObject RequestSettings(string subtype = null);

        /// <inheritdoc cref="WriteSettings{T}"/>
        protected abstract void WriteSettings(JObject settings, string subtype = null);

        /// <inheritdoc cref="RequestSlotSetting{T}"/>
        protected abstract JObject RequestSlotSetting(Guid slot);

        /// <inheritdoc cref="RequestSlotSettings{T}"/>
        protected abstract IEnumerable<(Guid id, JObject config)> RequestSlotSettings();

        /// <inheritdoc cref="WriteSlotSettings{T}"/>
        protected abstract void WriteSlotSettings(Guid id, JObject config);

        private void OnSettingsChanged(string obj) {
            SettingsChanged?.Invoke(obj);
        }

        private void OnSlotConfigChanged(Guid obj) {
            SlotConfigChanged?.Invoke(obj);
        }
    }
}