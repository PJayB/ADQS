using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AudioSwitchCommon
{
    public class KeyMonitor : IDisposable
    {
        private static class WorkerThread
        {
            private static List<KeyMonitor> s_instances = new List<KeyMonitor>();
            private static ConcurrentQueue<KeyEventArgs> s_events = new ConcurrentQueue<KeyEventArgs>();
            private static volatile bool s_running = false;
            private static Thread s_worker = null;
            private static ManualResetEvent s_wakeEvent = null;

            public static int InstanceCount => s_instances.Count;

            public static void Initialize()
            {
                if (s_running)
                    return;

                s_running = true;
                s_wakeEvent = new ManualResetEvent(false);
                s_worker = new Thread(WorkerThreadFunc);
                s_worker.SetApartmentState(ApartmentState.STA);
                s_worker.Start();
            }

            public static void Shutdown()
            {
                if (!s_running)
                    return;

                // notify the thread we're shutting down
                s_running = false;
                s_wakeEvent.Set();
                s_worker.Join();
                s_instances.Clear();
            }

            public static void PushEvent(KeyEventArgs e)
            {
                s_events.Enqueue(e);
                s_wakeEvent.Set();
                s_wakeEvent.Reset();
            }

            public static void AddMonitor(KeyMonitor monitor)
            {
                lock (s_instances)
                {
                    s_instances.Add(monitor);
                }
            }

            public static void RemoveMonitor(KeyMonitor monitor)
            {
                lock (s_instances)
                {
                    s_instances.Remove(monitor);
                }
            }

            static void WorkerThreadFunc()
            {
                while (true)
                {
                    // Wait for the event to fire
                    s_wakeEvent.WaitOne();

                    // If we're quitting, exit
                    if (!s_running)
                        return;

                    // Process the queue
                    ProcessQueue();
                }
            }

            static void ProcessQueue()
            {
                lock (s_instances)
                {
                    KeyEventArgs e;
                    while (s_events.TryDequeue(out e))
                    {
                        foreach (var instance in s_instances)
                        {
                            if (instance.Key == e.Key)
                            {
                                instance.FireEvent(e);
                            }
                        }
                    }
                }
            }
        }

        private static class LowLevelHook
        {
            private const int c_WH_KEYBOARD_LL = 13;
            private const int c_WM_KEYDOWN = 0x0100;
            private const int c_WM_KEYUP = 0x0101;
            private const int c_WM_SYSKEYDOWN = 0x0104;
            private const int c_WM_SYSKEYUP = 0x0105;

            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            private static IntPtr s_hookID;
            private static LowLevelKeyboardProc s_delegate;

            public static IntPtr ModuleHandle
            {
                get
                {
                    ProcessModule procModule = Process.GetCurrentProcess().MainModule;
                    return GetModuleHandle(procModule.ModuleName);
                }
            }

            public static void Initialize()
            {
                s_delegate = new LowLevelKeyboardProc(InternalCallBack);
                s_hookID = SetWindowsHookEx(c_WH_KEYBOARD_LL, s_delegate, ModuleHandle, 0);
            }

            public static void Shutdown()
            {
                UnhookWindowsHookEx(s_hookID);
                s_hookID = IntPtr.Zero;
            }

            private static IntPtr InternalCallBack(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode < 0)
                    return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
                
                Key key = KeyInterop.KeyFromVirtualKey(Marshal.ReadInt32(lParam));
                
                // Look up the correct monitor
                switch ((int)wParam)
                {
                    case c_WM_KEYDOWN:
                    case c_WM_SYSKEYDOWN:
                        ProcessHook(key, true);
                        break;
                    case c_WM_KEYUP:
                    case c_WM_SYSKEYUP:
                        ProcessHook(key, false);
                        break;
                }

                return CallNextHookEx(s_hookID, nCode, wParam, lParam);
            }

            public static void ProcessHook(Key key, bool down)
            {
                KeyEventArgs e = new KeyEventArgs();
                e.Key = key;
                e.Down = down;

                WorkerThread.PushEvent(e);
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
            
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }

        public readonly Key Key;

        public class KeyEventArgs
        {
            public Key Key;
            public bool Down;
        }

        public delegate void KeyEventHandler(object sender, KeyEventArgs e);

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;

        public bool IsDown { get { return Keyboard.IsKeyDown(Key); } }

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (!IsInitialized)
            {
                WorkerThread.Initialize();
                LowLevelHook.Initialize();
                IsInitialized = true;
            }
        }

        public static void Shutdown()
        {
            if (IsInitialized)
            {
                LowLevelHook.Shutdown();
                WorkerThread.Shutdown();
                IsInitialized = false;
            }
        }

        public KeyMonitor(Key key)
        {
            Key = key;

            if (!IsInitialized)
            {
                throw new InvalidOperationException("KeyMonitor is not initialized");
            }

            WorkerThread.AddMonitor(this);
        }

        public void Dispose()
        {
            WorkerThread.RemoveMonitor(this);
        }

        private void FireEvent(KeyEventArgs e)
        {
            if (e.Down && KeyDown != null)
                KeyDown(this, e);
            else if (KeyUp != null)
                KeyUp(this, e);
        }
    }
}
