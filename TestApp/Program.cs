using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interceptor;
using StreamDeck;
using StreamDeck.Services;

namespace TestApp {
    class Program {
        static void Main(string[] args) {
            var intercept = new StreamDeck.Services.Interceptor();
            var rawInput = new RawInput();
            var settings = new Settings();

            //var kbdsrv = new KeyboardService(intercept, rawInput, settings);

            rawInput.KeyPressed += arg =>
                Console.WriteLine(
                    $"{arg.KeyPressEvent.DeviceName}: {arg.KeyPressEvent.VKeyName} > {arg.KeyPressEvent.KeyPressState}");

            intercept.KeyPressed += eventArgs =>
                Console.WriteLine($"{eventArgs.DeviceId}: {eventArgs.Key} > {eventArgs.State}");

            rawInput.Start();
            intercept.Start();

            Thread.Sleep(1000);

            //for (int i = 0x70; i < 0x7f; i++) {
            //    Console.WriteLine($"{i.ToString("X")}:");
            //    intercept.SendKey((Keys)i, KeyState.Down, 1);
            //    intercept.SendKey((Keys)i, KeyState.Up, 1);
            //}

            for (int i = 0; i < 10; i++) {
                Console.WriteLine($"{i}:");
                intercept.SendKey((Keys)0x7e, KeyState.Up, i);
                Thread.Sleep(200);
            }



            Console.ReadLine();
        }
    }
}