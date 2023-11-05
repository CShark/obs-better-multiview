using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using BetterMultiview.Data;
using BetterMultiview.Data.Presets;
using ObsInterop;

namespace BetterMultiview.Dialogs
{
    public partial class ScenePresetSlot : UserControl {
        private static ObservableCollection<SceneMetadata> _sceneList = new();

        public ObservableCollection<SceneMetadata> SceneList => _sceneList;

        public static readonly DirectProperty<ScenePresetSlot, PresetScene> PresetSceneProperty =
            AvaloniaProperty.RegisterDirect<ScenePresetSlot, PresetScene>(nameof(PresetScene), slot => slot.PresetScene,
                (slot, val) => slot.PresetScene = val);

        private PresetScene _presetScene;

        public PresetScene PresetScene {
            get => _presetScene;
            set => SetAndRaise(PresetSceneProperty, ref _presetScene, value);
        }

        static unsafe ScenePresetSlot() {
            if (Multiview.ObsAvailable) {
                Obs.obs_enum_scenes(&EnumSceneCallback, null);
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe byte EnumSceneCallback(void* param, obs_source* source) {
            var uuid = Extensions.GetString(Obs.obs_source_get_uuid(source));
            var name = Extensions.GetString(Obs.obs_source_get_name(source));

            _sceneList.Add(new SceneMetadata(Guid.Parse(uuid), name));

            return 1;
        }

        public ScenePresetSlot() {
            InitializeComponent();
        }
    }
}