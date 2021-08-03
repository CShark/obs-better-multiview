using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StreamDeck.Services {
    public class ScenePreview {
        private readonly ObsWatchService _obs;

        public ScenePreview(ObsWatchService obs) {
            _obs = obs;
        }

        public byte[] GetSnapshot(string scene) {
            if (_obs.IsObsConnected) {
                if (scene == "(active)") scene = null;
                if (scene == "(preview)") scene = _obs.WebSocket.GetPreviewScene().Name;

                var img = _obs.WebSocket.TakeSourceScreenshot(scene, "jpg", null, -1, -1).ImageData;
                var matchGroups = Regex.Match(img, @"^data:((?<type>[\w\/]+))?;base64,(?<data>.+)$").Groups;
                var base64Data = matchGroups["data"].Value;
                var binData = Convert.FromBase64String(base64Data);

                return binData;
            }

            return null;
        }
    }
}