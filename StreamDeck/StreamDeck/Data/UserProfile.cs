using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamDeck.Data {
    public class UserProfile {
        public class DSlot {
            public string Name { get; set; }

            [JsonIgnore]
            public Guid Id { get; }

            public DSlotObs Obs { get; set; }

            public Dictionary<string, JObject> PluginConfigs { get; set; }

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

        public class DSlotObs {
            public string Scene { get; set; }
        }

        public class DSceneViewConfig {
            public int Rows { get; set; }
            public int Columns { get; set; }

            public List<DSlot> Slots { get; set; }

            public DSceneViewConfig() {
                Slots = new List<DSlot>();
            }
        }

        public class DObsProfile {
            public string Id { get; set; }
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

        public List<DObsProfile> Profiles { get; set; }

        [JsonIgnore]
        public string Name { get; set; }
    }
}