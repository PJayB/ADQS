using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioEndPointControllerWrapper;

namespace Prototype
{
    class Program
    {
        static void PrintAudioDevice(IAudioDevice device)
        {
            Console.WriteLine("  " + device.FriendlyName);
        }

        static void Main(string[] args)
        {
            var allPlaybackDevices = AudioController.GetAllPlaybackDevices();
            var activePlaybackDevices = AudioController.GetActivePlaybackDevices();
            var allRecordingDevices = AudioController.GetAllRecordingDevices();
            var activeRecordingDevices = AudioController.GetActiveRecordingDevices();

            Console.WriteLine("All Playback Devices:");
            foreach (var device in allPlaybackDevices)
                PrintAudioDevice(device);
            
            Console.WriteLine("Active Playback Devices:");
            foreach (var device in activePlaybackDevices)
                PrintAudioDevice(device);

            Console.WriteLine("All Recording Devices:");
            foreach (var device in allRecordingDevices)
                PrintAudioDevice(device);

            Console.WriteLine("Active Recording Devices:");
            foreach (var device in activeRecordingDevices)
                PrintAudioDevice(device);
        }
    }
}
