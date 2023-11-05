using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObsInterop;

namespace BetterMultiview.Parts
{
    internal class Item {
        public uint Row { get; set; }
        public uint Column { get; set; }
        public uint RowSpan { get; set; } = 1;
        public uint ColSpan { get; set; } = 1;

        public unsafe void Render(uint x, uint y, uint width, uint height) {
            ObsGraphics.gs_matrix_push();
            ObsGraphics.gs_matrix_translate3f(x, y, 0);

            DrawUtils.DrawBox(width, height, Colors.Black);

            obs_video_info ovi = new obs_video_info();
            Obs.obs_get_video_info(&ovi);

            ObsGraphics.gs_matrix_scale3f((float)width / ovi.base_width, (float)height / ovi.base_height, 1);

            Obs.obs_render_main_texture();

            ObsGraphics.gs_matrix_pop();
        }
    }
}
