using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using BetterMultiview.Desktop;
using BetterMultiview.Dialogs;
using BetterMultiview.Parts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using BetterMultiview.Data;
using ObsInterop;
using BetterMultiview.Data.Presets;
using BetterMultiview.Data.View;

namespace BetterMultiview {
    public unsafe partial class Multiview : Window {
        private readonly bool _obs = false;
        private obs_display* _display = null;
        private Rect _selection = new Rect();

        private static Multiview? _instance;

        private Configuration _configuration = new() { Rows = 5, Columns = 4 };

        public static Multiview? Instance => _instance;

        public static bool ObsAvailable => Instance?._obs ?? false;

        public ObservableCollection<ViewItem> Items { get; set; } = new();

        public Multiview() {
            _instance = this;
            Plugin.Log("Multiview loading");
            InitializeComponent();

            Closed += Multiview_Closed;
            Opened += Multiview_Opened;
            Resized += Multiview_Resized;

            try {
                Obs.obs_get_version();
                _obs = true;
            } catch {
                _obs = false;
                Background = Brushes.Black;
            }

            PresetFactory.RegisterPreset<PresetScene>();

            var bounds = GetBounds();
            WorkingArea.Width = bounds.Width;
            WorkingArea.Height = bounds.Height;
        }

        private void Multiview_Resized(object? sender, WindowResizedEventArgs e) {
            if (_display != null && _obs) {
                Obs.obs_display_resize(_display, (uint)ClientSize.Width, (uint)ClientSize.Height);

                var bounds = GetBounds();
                WorkingArea.Width = bounds.Width;
                WorkingArea.Height = bounds.Height;
            }
        }

        private void Multiview_Opened(object? sender, System.EventArgs e) {
            if (Design.IsDesignMode || !_obs) return;

            // Needs to be retested for Linux & MacOS
            var hwnd = TryGetPlatformHandle();
            var init = new gs_init_data {
                window = new gs_window { hwnd = (void*)hwnd.Handle },
                format = gs_color_format.GS_BGRA,
                zsformat = gs_zstencil_format.GS_ZS_NONE,
                cx = (uint)Width,
                cy = (uint)Height
            };

            _display = Obs.obs_display_create(&init, 0);
            Obs.obs_display_add_draw_callback(_display, &Render, null);
            Obs.obs_display_set_background_color(_display, 0);
        }

        private void Multiview_Closed(object? sender, System.EventArgs e) {
            if (Design.IsDesignMode || _obs) return;

            if (_display != null) {
                Obs.obs_display_destroy(_display);
                _display = null;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static void Render(void* param, uint cx, uint cy) {
            _instance.RenderInstance(param, cx, cy);
        }

        private void RenderInstance(void* param, uint cx, uint cy) {
            try {
                var bounds = GetBounds();

                ObsGraphics.gs_matrix_push();
                ObsGraphics.gs_matrix_translate3f((uint)bounds.X, (uint)bounds.Y, 0);

                // Draw basic empty boxes
                DrawUtils.DrawBox((uint)bounds.Width, (uint)bounds.Height, Colors.Gray);

                var cellWidth = bounds.Width / _configuration.Columns;
                var cellHeight = bounds.Height / _configuration.Rows;

                for (uint x = 0; x < _configuration.Columns; x++) {
                    for (uint y = 0; y < _configuration.Rows; y++) {
                        var area = GridToScreenArea(new Point(x, y));
                        DrawUtils.DrawBox(area, Colors.Black);
                    }
                }

                foreach (var item in Items) {
                    var gridArea = GridToScreenArea(item.Position);
                    obs_video_info ovi = new obs_video_info();
                    Obs.obs_get_video_info(&ovi);

                    DrawUtils.DrawBox(gridArea, Colors.Black);

                    ObsGraphics.gs_matrix_push();
                    ObsGraphics.gs_matrix_translate3f((float)gridArea.X, (float)gridArea.Y, 0);
                    ObsGraphics.gs_matrix_scale3f((float)gridArea.Width / ovi.base_width, (float)gridArea.Height / ovi.base_height, 1);

                    item.Render();

                    ObsGraphics.gs_matrix_pop();
                }

                if (_selection is { Width: > 0, Height: > 0 }) {
                    var gridArea = GridToScreenArea(_selection);
                    DrawUtils.DrawBox(gridArea, Color.FromArgb(64, 0, 128, 255));
                }

                ObsGraphics.gs_matrix_pop();
            } catch (Exception ex) {
                Trace.WriteLine("Error while Rendering: " + ex);
            }
        }

        private void WorkingArea_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            var pos = e.GetCurrentPoint(this);

            if (pos.Properties.IsRightButtonPressed) {
                var gridPos = ScreenToGrid(pos.Position);
                _selection = new Rect(gridPos, new Size(1, 1));
            } else {
                _selection = new Rect();
            }
        }

        private void WorkingArea_OnPointerMoved(object? sender, PointerEventArgs e) {
            if (_selection is { Width: > 0, Height: > 0 }) {
                var pos = e.GetCurrentPoint(this);

                if (pos.Properties.IsRightButtonPressed) {
                    var gridPos = ScreenToGrid(pos.Position);

                    var min = new Point(
                        Math.Min(_selection.X, gridPos.X),
                        Math.Min(_selection.Y, gridPos.Y)
                    );

                    var size = new Size(
                        Math.Max(_selection.X, gridPos.X) - min.X + 1,
                        Math.Max(_selection.Y, gridPos.Y) - min.Y + 1
                    );

                    _selection = new Rect(min, size);
                }
            }
        }

        #region Helpers

        private IEnumerable<ViewItem> GetViewItemsAtGrid(Rect area) {
            foreach (var item in Items) {
                if (area.Intersects(item.Position)) {
                    yield return item;
                }
            }
        }

        private Rect GridToScreenArea(Point point) => GridToScreenArea(new Rect(point, new Size(1, 1)));

        private Rect GridToScreenArea(Rect point) {
            var bounds = GetBounds();
            var cellWidth = bounds.Width / _configuration.Columns;
            var cellHeight = bounds.Height / _configuration.Rows;

            return new Rect((uint)(point.X * cellWidth + 4),
                (uint)(point.Y * cellHeight + 4),
                (uint)(point.Width * cellWidth - 8), (uint)(point.Height * cellHeight - 8));
        }

        private Point ScreenToGrid(Point point) {
            var bounds = GetBounds();

            var cellWidth = bounds.Width / _configuration.Columns;
            var cellHeight = bounds.Height / _configuration.Rows;

            var x = (point.X - bounds.X) / cellWidth;
            var y = (point.Y - bounds.Y) / cellHeight;

            return new Point((uint)Math.Clamp(x, -1, _configuration.Columns),
                (uint)Math.Clamp(y, -1, _configuration.Rows));
        }

        private Rect GetBounds() {
            var ax = ClientSize.Width / (16f * _configuration.Columns);
            var ay = (ClientSize.Height - 50) / (9f * _configuration.Rows);

            var width = ClientSize.Width;
            var height = ClientSize.Height - 50;

            if (ax < ay) {
                height = (uint)(ax * (9 * _configuration.Rows) + 8);
            } else {
                width = (uint)(ay * (16 * _configuration.Columns) + 8);
            }

            return new((ClientSize.Width - width) / 2, (ClientSize.Height - 50 - height) / 2, width, height);
        }

        #endregion

        private void CreateLiveView_OnClick(object? sender, RoutedEventArgs e) {
            if (_selection is { Width: > 0, Height: > 0 }) {
                var items = GetViewItemsAtGrid(_selection).ToList();
                foreach (var item in items) {
                    Items.Remove(item);
                }

                Items.Add(new LiveViewItem(_selection));
            }

            _selection = new();
        }

        private void ClearSelection_OnClick(object? sender, RoutedEventArgs e) {
            if (_selection is { Width: > 0, Height: > 0 }) {
                var items = GetViewItemsAtGrid(_selection).ToList();
                foreach (var item in items) {
                    Items.Remove(item);
                }
            }

            _selection = new();
        }
    }
}