using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using BetterMultiview.Parts;

namespace BetterMultiview.Data.View {
    public abstract class ViewItem {
        public Rect Position { get; set; }

        public abstract void Render();

        protected ViewItem(Rect position) {
            Position = position;
        }
    }
}