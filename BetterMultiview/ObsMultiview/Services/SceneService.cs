using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ObsMultiview.Data;
using ObsMultiview.Extensions;
using ObsMultiview.Plugins;

namespace ObsMultiview.Services {
    /// <summary>
    /// Interacts with scenes in OBS
    /// </summary>
    public class SceneService {
        private readonly ObsWatchService _obs;
        private readonly ProfileWatcher _profile;
        private readonly ILogger _logger;
        private readonly PluginService _plugins;

        private UserProfile.DSlot _previewScene;
        private UserProfile.DSlot _liveScene;

        private NullDictionary<PluginBase, (IPluginSettingsProvider set, IPluginSettingsProvider slot)> _activeSettings =
            new();

        /// <summary>
        /// Fired when the active preview changes
        /// </summary>
        /// <remarks>Internal only, not synced with OBS because one scene in OBS can be associated with multiple slots</remarks>
        public event Action<Guid> PreviewChanged;

        /// <summary>
        /// Fired when the live scene changes
        /// </summary>
        public event Action<Guid> LiveChanged;

        /// <summary>
        /// Currently active preview slot
        /// </summary>
        public UserProfile.DSlot ActivePreviewSlot => _previewScene;

        /// <summary>
        /// Currently active live slot
        /// </summary>
        public UserProfile.DSlot ActiveLiveSlot => _liveScene;

        public SceneService(ObsWatchService obs, ProfileWatcher profile, ILogger<SceneService> logger,
            PluginService plugins) {
            _obs = obs;
            _profile = profile;
            _logger = logger;
            _plugins = plugins;

            _profile.ActiveProfileChanged += obsProfile => { OnPreviewChanged(null); };
        }

        /// <summary>
        /// Switch the preview and live scenes
        /// </summary>
        public void SwitchLive() {
            _logger.LogDebug("Switching preview to live");
            var temp = _liveScene;

            UnapplyScene(_liveScene, _previewScene);
            OnLiveChanged(_previewScene);
            OnPreviewChanged(temp);
            ApplyScene(_liveScene, _previewScene);
        }

        /// <summary>
        /// Activate a scene for preview based on its guid
        /// </summary>
        /// <param name="id"></param>
        public void ActivatePreview(Guid id) {
            _logger.LogDebug($"Activating preview {id}");
            var scene = _profile.ActiveProfile.SceneView.Slots.FirstOrDefault(x => x.Id == id);

            if (scene != null) {
                OnPreviewChanged(scene);
            }
        }

        protected virtual void OnPreviewChanged(UserProfile.DSlot obj) {
            _previewScene = obj;
            PreviewChanged?.Invoke(obj?.Id ?? Guid.Empty);
        }

        protected virtual void OnLiveChanged(UserProfile.DSlot obj) {
            _liveScene = obj;
            LiveChanged?.Invoke(obj?.Id ?? Guid.Empty);
        }

        /// <summary>
        /// Called to unload & clean up in preparation for a scene change
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="next"></param>
        private void UnapplyScene(UserProfile.DSlot slot, UserProfile.DSlot next) {
            _logger.LogDebug($"Unapplying scene {slot?.Name} to {next?.Name}");

            foreach (var plugin in
                     _plugins.Plugins.Where(x => x.Active && x.Plugin.TriggerType == PluginTriggerType.Change)) {
                if (plugin.Plugin is ChangePluginBase cplug) {
                    var config = ResolveActiveSettings(cplug, next);

                    // first exit slot, then set
                    if (config.slot != _activeSettings?[cplug].slot) {
                        try {
                            cplug.OnSlotExit(_activeSettings?[cplug].slot?.GetPluginSettings(cplug.Name),
                                config.slot?.GetPluginSettings(cplug.Name));
                        } catch (Exception ex) {
                            _logger.LogError(ex, $"Failed to trigger SlotExit for plugin {cplug.Name}");
                        }
                    }

                    if (config.set != _activeSettings?[cplug].set) {
                        try {
                            cplug.OnSlotExit(_activeSettings?[cplug].set?.GetPluginSettings(cplug.Name),
                                config.set?.GetPluginSettings(cplug.Name));
                        } catch (Exception ex) {
                            _logger.LogError(ex, $"Failed to trigger SetExit for plugin {cplug.Name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called to apply a scene config
        /// </summary>
        /// <param name="slot"></param>
        private void ApplyScene(UserProfile.DSlot slot, UserProfile.DSlot prev) {
            _logger.LogDebug($"Applying scene {slot?.Name}");

            if (!string.IsNullOrEmpty(slot?.Obs.Scene)) {
                if (_obs.WebSocket.GetSceneList().Scenes.Any(x => x.Name == slot.Obs.Scene)) {
                    _obs.WebSocket.SetCurrentScene(slot.Obs.Scene);
                } else {
                    _logger.LogError("Scene does not exist in OBS");
                }
            }

            foreach (var plugin in
                     _plugins.Plugins.Where(x => x.Active && x.Plugin.TriggerType == PluginTriggerType.Change)) {
                if (plugin.Plugin is ChangePluginBase cplug) {
                    var config = ResolveActiveSettings(cplug, slot);

                    // first enter set, then slot
                    if (config.set != _activeSettings?[cplug].set) {
                        try {
                            cplug.OnSlotEnter(config.set?.GetPluginSettings(cplug.Name),
                                _activeSettings?[cplug].set?.GetPluginSettings(cplug.Name));
                        } catch (Exception ex) {
                            _logger.LogError(ex, $"Failed to trigger SlotEnter for plugin {cplug.Name}");
                        }
                    }

                    if (config.slot != _activeSettings?[cplug].slot) {
                        try {
                            cplug.OnSlotEnter(config.slot?.GetPluginSettings(cplug.Name),
                                _activeSettings?[cplug].slot?.GetPluginSettings(cplug.Name));
                        } catch (Exception ex) {
                            _logger.LogError(ex, $"Failed to trigger SlotEnter for plugin {cplug.Name}");
                        }
                    }

                    _activeSettings[cplug] = config;
                }
            }

            foreach (var plugin in
                     _plugins.Plugins.Where(x => x.Active && x.Plugin.TriggerType == PluginTriggerType.State)) {
                if (plugin.Plugin is StatePluginBase splug) {
                    var config = ResolveActiveSettings(splug, slot);

                    try {
                        splug.ActiveSettingsChanged((config.slot ?? config.set)?.GetPluginSettings(splug.Name),
                            (_activeSettings?[splug].slot ?? _activeSettings?[splug].set)
                            ?.GetPluginSettings(splug.Name));
                    } catch (Exception ex) {
                        _logger.LogError(ex, $"Failed to apply slot state for plugin {splug.Name}");
                    }

                    _activeSettings[splug] = config;
                }
            }
        }

        private (IPluginSettingsProvider set, IPluginSettingsProvider slot) ResolveActiveSettings(PluginBase plugin,
            UserProfile.DSlot slot) {
            if (slot == null) return (null, null);
            (IPluginSettingsProvider set, IPluginSettingsProvider slot) result = (null, null);

            if (!_activeSettings.ContainsKey(plugin))
                _activeSettings.Add(plugin, (null, null));


            if (slot.PluginConfigs.ContainsKey(plugin.Name) && slot.PluginConfigs[plugin.Name] != null) {
                result.slot = slot;
            }

            if (slot.SetId != null) {
                var set = _profile.ActiveProfile.SceneView.Sets.FirstOrDefault(x => x.Id == slot.SetId);

                if (set != null && set.Id == Guid.Empty) {
                    _activeSettings.TryGetValue(plugin, out var config);
                    result.set = config.set;
                } else if (set != null && set.PluginConfigs.ContainsKey(plugin.Name) &&
                           set.PluginConfigs[plugin.Name] != null) {
                    result.set = set;
                }
            }

            return result;
        }

        public void ClearPreview() {
            OnPreviewChanged(null);
        }

        public void ClearLive() {
            OnLiveChanged(null);
        }
    }
}