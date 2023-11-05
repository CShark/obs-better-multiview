using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace BetterMultiview.Data.Presets
{
    public abstract class PresetBase
    {
        public int SlotId { get; set; }

        public string Title { get; set; }

        public string Tooltip { get; set; }

        public abstract UserControl CreateEditor();

        public abstract PresetBase Clone();

        protected void CloneInto(PresetBase target)
        {
            target.SlotId = SlotId;
            target.Title = Title;
            target.Tooltip = Tooltip;
        }
    }
}
