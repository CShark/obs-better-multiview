using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using BetterMultiview.Parts;
using ObsInterop;

namespace BetterMultiview.Data.View {
    internal class LiveViewItem : ViewItem {
        public override void Render() {
            Obs.obs_render_main_texture();
        }

        public LiveViewItem(Rect position) : base(position) {
        }
    }
}