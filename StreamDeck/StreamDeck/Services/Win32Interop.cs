using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeck.Data;

namespace StreamDeck.Services
{
    public class Win32Interop {
        private readonly Settings _settings;

        private static extern bool EnumThreadWindows();

        public Win32Interop(Settings settings) {
            _settings = settings;
        }


    }
}
