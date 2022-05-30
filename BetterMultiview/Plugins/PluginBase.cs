using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ObsMultiview.Plugins {
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
    /// The type of change the plugin gets triggered by
    /// </summary>
    public enum PluginTriggerType {
        /// <summary>
        /// Is not triggered by slots but instead triggers a slot
        /// </summary>
        Trigger,

        /// <summary>
        /// Gets triggered when a slot is activated
        /// </summary>
        State,

        /// <summary>
        /// Gets triggered when a slot gets activated or deactivated
        /// </summary>
        Change
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
            nameof(Settings), typeof(T), typeof(SettingsControl<T>), new PropertyMetadata(default(T)));

        /// <summary>
        /// The global settings of this plugin
        /// </summary>
        public T Settings {
            get { return (T)GetValue(SettingsProperty); }
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
            nameof(Settings), typeof(T), typeof(SlotSettingsControl<T>), new PropertyMetadata(default(T)));

        public T Settings {
            get { return (T)GetValue(SettingsProperty); }
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

        protected SlotSettingsControl(CommandFacade commandFacade, Guid? slotID) {
            if(slotID == null) throw new ArgumentNullException(nameof(slotID));

            CommandFacade = commandFacade;
            SlotID = slotID.Value;
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
        /// The trigger type of this plugin
        /// </summary>
        public abstract PluginTriggerType TriggerType { get; }

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
        /// Whether this plugin is in experimental stages
        /// </summary>
        public virtual bool IsExperimental { get; } = false;

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
        public abstract SettingsControl GetSlotSettings(Guid? slot);

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
    /// Plugin base for Trigger-Type plugins
    /// </summary>
    public abstract class TriggerPluginBase : PluginBase {
        public sealed override PluginTriggerType TriggerType => PluginTriggerType.Trigger;
    }

    public abstract class StatePluginBase : PluginBase {
        public sealed override PluginTriggerType TriggerType => PluginTriggerType.State;

        public abstract void ActiveSettingsChanged(JObject next, JObject previous);

        public abstract void PrepareSettings(JObject preview, JObject live);
    }

    /// <summary>
    /// Plugin base for State-Type plugins
    /// </summary>
    public abstract class StatePluginBase<T> : StatePluginBase where T : class {
        public override void ActiveSettingsChanged(JObject next, JObject previous) {
            var nextT = next?.ToObject<T>();
            var prevT = previous?.ToObject<T>();

            if (nextT != null && !nextT.Equals(prevT)) {
                ActiveSettingsChanged(nextT);
            }
        }

        public override void PrepareSettings(JObject preview, JObject live) {
            PrepareSettings(preview?.ToObject<T>(), live?.ToObject<T>());
        }

        /// <summary>
        /// Gets triggered when the live-slot changes
        /// </summary>
        /// <param name="slot">The name of the slot</param>
        protected abstract void ActiveSettingsChanged(T slot);

        /// <summary>
        /// Gets triggered when the preview slot changes to prepare for transitions
        /// </summary>
        /// <param name="preview"></param>
        /// <param name="live"></param>
        protected abstract void PrepareSettings(T preview, T live);
    }

    public abstract class ChangePluginBase : PluginBase {
        public sealed override PluginTriggerType TriggerType => PluginTriggerType.Change;

        public abstract void OnSlotExit(JObject slot, JObject next);
        public abstract void OnSlotEnter(JObject slot, JObject previous);
    }

    public abstract class ChangePluginBase<T> : ChangePluginBase where T : class {
        public sealed override void OnSlotExit(JObject slot, JObject next) {
            OnSlotExit(slot?.ToObject<T>(), next?.ToObject<T>());
        }

        public sealed override void OnSlotEnter(JObject slot, JObject previous) {
            OnSlotEnter(slot?.ToObject<T>(), previous?.ToObject<T>());
        }

        /// <summary>
        /// Triggered when exiting a live slot
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="next"></param>
        protected abstract void OnSlotExit(T slot, T next);

        /// <summary>
        /// Triggered when entering a new slot
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="previous"></param>
        protected abstract void OnSlotEnter(T slot, T previous);
    }
}