using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Data
{
    public class Settings
    {
        public struct SConnection {
            public string IP { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }
        }

        public SConnection Connection { get; set; }
    }
}
