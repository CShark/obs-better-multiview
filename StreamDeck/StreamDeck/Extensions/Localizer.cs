using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WPFLocalizeExtension.Engine;

namespace StreamDeck.Extensions {
    static class Localizer {
        public static T Localize<T>(string dictionary, string key) {
            var asm = Assembly.GetCallingAssembly().FullName;

            return (T)LocalizeDictionary.Instance.GetLocalizedObject(asm, dictionary, key,
                LocalizeDictionary.Instance.Culture);
        }
    }
}