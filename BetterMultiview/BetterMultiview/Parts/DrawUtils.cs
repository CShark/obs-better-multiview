using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using BetterMultiview.Data;
using ObsInterop;
using Effect = Avalonia.Media.Effect;

namespace BetterMultiview.Parts
{
    internal static unsafe class DrawUtils {
        public static void DrawBox(Rect rect, Color color) {
            DrawBox((uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height, color);
        }

        public static void DrawBox(uint width, uint height, Color color) {
            if (width > 0 && height > 0) {
                gs_effect* effect = Obs.obs_get_base_effect(obs_base_effect.OBS_EFFECT_SOLID);
                gs_effect_param* param;
                var text = Encoding.UTF8.GetBytes("color").Select(x => (sbyte)x).ToArray();
                fixed (sbyte* textPtr = text) {
                    param = ObsGraphics.gs_effect_get_param_by_name(effect, textPtr);
                }

                ObsGraphics.gs_effect_set_color(param, color.ToUInt32());
                fixed (sbyte* textPtr = "Solid".GetBytes()) {
                    while (ObsGraphics.gs_effect_loop(effect, textPtr) != 0) {
                        ObsGraphics.gs_draw_sprite(null, 0, width, height);
                    }
                }
            }
        }

        public static void DrawBox(uint x, uint y, uint width, uint height, Color color) {
            ObsGraphics.gs_matrix_push();
            ObsGraphics.gs_matrix_translate3f(x, y, 0);
            DrawBox(width, height, color);
            ObsGraphics.gs_matrix_pop();
        }
    }
}