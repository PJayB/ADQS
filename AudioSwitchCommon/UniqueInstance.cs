using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioSwitchCommon
{
    public class UniqueInstance : IDisposable
    {
        Mutex _mutex;

        private UniqueInstance(Mutex mutex)
        {
            _mutex = mutex;
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex = null;
            }
        }

        public static UniqueInstance Acquire(string identifier)
        {
            Mutex mutex = new Mutex(true, identifier);
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                return new UniqueInstance(mutex);
            }
            else
            {
                throw new ApplicationException("Application already running.");
            }
        }
    }
}
