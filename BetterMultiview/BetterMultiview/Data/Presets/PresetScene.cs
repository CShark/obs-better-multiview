using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using BetterMultiview.Dialogs;

namespace BetterMultiview.Data.Presets
{
    public class PresetScene : PresetBase
    {
        public string ObsSceneUUID { get; set; }


        public override UserControl CreateEditor()
        {
            return new ScenePresetSlot { PresetScene = this };
        }

        public override PresetScene Clone()
        {
            var clone = new PresetScene();
            CloneInto(clone);
            clone.ObsSceneUUID = ObsSceneUUID;
            return clone;
        }
    }
}
