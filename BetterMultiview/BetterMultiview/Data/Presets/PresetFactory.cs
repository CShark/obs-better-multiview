using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterMultiview.Data.Presets;

namespace BetterMultiview.Data.Presets
{
    internal static class PresetFactory
    {
        private static Dictionary<string, Func<PresetBase>> _generators = new();

        public static void RegisterPreset<T>() where T : PresetBase, new()
        {
            _generators[typeof(T).Name] = () => new T();
        }

        public static bool PresetRegistered(string type)
        {
            return _generators.ContainsKey(type);
        }

        public static PresetBase CreateInstance(string type)
        {
            if (_generators.ContainsKey(type))
            {
                return _generators[type]();
            }
            else
            {
                throw new ArgumentException($"Type {type} not registered");
            }
        }
    }
}