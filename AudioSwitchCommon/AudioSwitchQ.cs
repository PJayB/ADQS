using AudioEndPointControllerWrapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADQSCommon
{
    public class AudioSwitchQ : IDisposable
    {
        Thread _thread;
        volatile bool _running;
        ConcurrentQueue<IAudioDevice> _switchQ;
        ManualResetEvent _go;

        public AudioSwitchQ()
        {
            _running = true;
            _switchQ = new ConcurrentQueue<IAudioDevice>();
            _go = new ManualResetEvent(false);
            _thread = new Thread(WorkerThreadFunc);
            _thread.Start();
        }

        public void Dispose()
        {
            _running = false;
            _go.Set();
            _thread.Join();
        }

        private void WorkerThreadFunc()
        {
            while (_running)
            {
                // Wait for input
                _go.WaitOne();

                // Dequeue the devices
                IAudioDevice device = null, tmp;
                while (_switchQ.TryDequeue(out tmp))
                {
                    // Ignore every one except the last one
                    device = tmp;
                }

                if (device != null && device.Type == AudioDeviceType.Playback && device.DeviceState == DeviceState.Active)
                {
                    device.SetAsDefault(Role.Multimedia);
                }
            }
        }

        public void Push(IAudioDevice device)
        {
            _switchQ.Enqueue(device);
            _go.Set();
            _go.Reset();
        }
    }
}
