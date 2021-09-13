using System.Reflection;
using WPFLocalizeExtension.Engine;

namespace ObsMultiview.Extensions {
    static class Localizer {
        public static T Localize<T>(string dictionary, string key) {
            var asm = Assembly.GetCallingAssembly().FullName;

            return (T)LocalizeDictionary.Instance.GetLocalizedObject(asm, dictionary, key,
                LocalizeDictionary.Instance.Culture);
        }
    }
}