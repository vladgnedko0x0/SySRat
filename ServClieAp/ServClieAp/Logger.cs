using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace ServClieAp
{
    internal class Logger
    {
        private static readonly object lockObj = new object();
        private static InputSimulator inputSimulator = new InputSimulator();

        Logger() { }

        public static async Task ReaderKeys()
        {
            await Task.Run(() => StartReading());
        }

        private static void StartReading()
        {
            while (true)
            {
                if (inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.CONTROL)
                    && inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.VK_C))
                {
                    Console.WriteLine("Ctrl+C pressed. Copy action detected.");
                    // Actions related to copying
                }
                else if (inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.CONTROL)
                    && inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.VK_X))
                {
                    Console.WriteLine("Ctrl+X pressed. Cut action detected.");
                    // Actions related to cutting
                }
                else if (inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.CONTROL)
                    && inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.VK_V))
                {
                    Console.WriteLine("Ctrl+V pressed. Paste action detected.");
                    // Actions related to pasting
                }

                // Sleep to reduce CPU usage
                Task.Delay(50).Wait();
            }
        }
    }
}
