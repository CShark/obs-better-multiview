﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace StreamDeck.Data {
    /// <summary>
    /// Contains general settings for the stream deck
    /// </summary>
    public class Settings {
        /// <summary>
        /// Connection infos to OBS Websocket
        /// </summary>
        public struct DConnection {
            public string IP { get; set; }
            public int Port { get; set; }
            public string Password { get; set; }

            /// <summary>
            /// The process name of obs used to find its window
            /// </summary>
            public string Process { get; set; }
        }

        /// <summary>
        /// The index of the screen on which to show the custom multiview
        /// </summary>
        public int Screen { get; set; }

        /// <summary>
        /// Connection infos to OBS Websocket
        /// </summary>
        public DConnection Connection { get; set; }

        /// <summary>
        /// The last active profile
        /// </summary>
        public string LastProfile { get; set; }

        /// <summary>
        /// A list of activated plugins
        /// </summary>
        public HashSet<string> ActivePlugins { get; set; } = new();

        /// <summary>
        /// A list of global plugin settings
        /// </summary>
        public Dictionary<string, JObject> PluginSettings { get; set; } = new();
    }
}