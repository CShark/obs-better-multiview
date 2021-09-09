using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamDeck.Data {
    /// <summary>
    /// A single user profile
    /// </summary>
    public class UserProfile {
        /// <summary>
        /// Config options for a single slot
        /// </summary>
        public class DSlot {
            /// <summary>
            /// Name of the slot
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// internal id of the slot. Not persistent during restarts
            /// </summary>
            [JsonIgnore]
            public Guid Id { get; }

            /// <summary>
            /// OBS config
            /// </summary>
            public DSlotObs Obs { get; set; }

            /// <summary>
            /// Plugin configs
            /// </summary>
            public Dictionary<string, JObject> PluginConfigs { get; set; } = new();

            public DSlot() {
                Obs = new DSlotObs();
                Id = Guid.NewGuid();
            }

            public static bool operator ==(DSlot a, DSlot b) {
                return a?.Id == b?.Id && a != null && b != null;
            }

            public static bool operator !=(DSlot a, DSlot b) {
                return a?.Id != b?.Id || (a == null && b == null);
            }
        }

        /// <summary>
        /// OBS configuration for the slot
        /// </summary>
        public class DSlotObs {
            /// <summary>
            /// Name of the scene associated with this slot
            /// </summary>
            public string Scene { get; set; }
        }

        /// <summary>
        /// Configuration for the scene view of a specific scene collection
        /// </summary>
        public class DSceneViewConfig {
            /// <summary>
            /// Number of Rows
            /// </summary>
            public int Rows { get; set; }

            /// <summary>
            /// Number of Columns
            /// </summary>
            public int Columns { get; set; }

            public List<DSlot> Slots { get; set; }

            public DSceneViewConfig() {
                Slots = new List<DSlot>();
            }
        }

        /// <summary>
        /// Configuration for a specific scene collection
        /// </summary>
        public class DObsProfile {
            /// <summary>
            /// ID of the scene collection
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Configuration for this scene collection
            /// </summary>
            public DSceneViewConfig SceneView { get; set; }

            public DObsProfile() {
            }

            public DObsProfile(string id) {
                Id = id;
                SceneView = new DSceneViewConfig();
            }

            public DObsProfile(string id, int rows, int columns) {
                Id = id;
                SceneView = new DSceneViewConfig {Rows = rows, Columns = columns};
            }
        }

        /// <summary>
        /// List of configurations for different scene collections
        /// </summary>
        public List<DObsProfile> Profiles { get; set; }

        /// <summary>
        /// Name of the profile, based on filename
        /// </summary>
        [JsonIgnore]
        public string Name { get; set; }
    }
}