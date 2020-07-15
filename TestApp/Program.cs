using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interceptor;

namespace TestApp {
    class Program {
        static void Main(string[] args) {
            var input = new Input();
            input.KeyboardFilterMode = KeyboardFilterMode.KeyDown;
            input.Load();

            input.OnKeyPressed += (sender, eventArgs) => {
                Console.WriteLine($"{eventArgs.DeviceId}:{eventArgs.Key.ToString()}");
            };

            Console.ReadLine();
            input.Unload();
        }
    }
}
