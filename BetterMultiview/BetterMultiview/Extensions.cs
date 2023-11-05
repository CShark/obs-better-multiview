using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Core.Plugins;

namespace BetterMultiview {
    internal static class Extensions {
        public static sbyte[] GetBytes(this string self) {
            return Encoding.UTF8.GetBytes(self).Select(x => (sbyte)x).ToArray();
        }

        public static unsafe string GetString(sbyte* data) {
            var result = new StringBuilder();

            if (data != null) {
                while (*data != 0) {
                    result.Append((char)*data);
                    data++;
                }
            }

            return result.ToString();
        }
    }
}