using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ObsMultiview.Plugins {
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
        public T RequestSlotSetting<T>(Guid? slot) {
            if (slot == null) return default(T);

            var json = RequestSlotSetting(slot.Value);

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