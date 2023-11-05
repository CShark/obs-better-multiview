using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterMultiview.Data {
    public class SceneMetadata {
        public Guid UUID { get; }
        public string Name { get; }

        public SceneMetadata(Guid uuid, string name) {
            UUID = uuid;
            Name = name;
        }
    }
}