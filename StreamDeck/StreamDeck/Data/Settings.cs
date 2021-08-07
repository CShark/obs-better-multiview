using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeck.Data {
    public class Settings {
        public struct DConnection {
            public string IP { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }

            public string Process { get; set; }
        }

        public int Screen { get; set; }
        
        public DConnection Connection { get; set; }

        public string LastProfile { get; set; }
    }
}