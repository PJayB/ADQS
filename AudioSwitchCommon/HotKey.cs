using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace ADQSCommon
{
    public class HotKey : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [Flags] public enum ModifierFlags
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        public readonly Key Key;
        public readonly ModifierFlags Modifiers;

        private readonly Action<HotKey> _callback;
        private readonly int _id;

        static private int s_globalHotKeyID = 0;
        static private Dictionary<int, HotKey> s_hotkeyLookupTable = new Dictionary<int, HotKey>();

        const int c_WM_HOTKEY = 0x0312;

        static HotKey()
        {
            ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
        }

        public HotKey(Key key, ModifierFlags modifiers, Action<HotKey> callback)
        {
            Key = key;
            Modifiers = modifiers;
            _callback = callback;
            _id = IssueGlobalID();

            int vcc = KeyInterop.VirtualKeyFromKey(Key);
            if (!RegisterHotKey(IntPtr.Zero, _id, (UInt32)modifiers, (UInt32)vcc))
            {
                throw new Exception("Couldn't bind hotkey");
            }

            s_hotkeyLookupTable.Add(_id, this);
        }

        public void Dispose()
        {
            UnregisterHotKey(IntPtr.Zero, _id);
        }

        static int IssueGlobalID()
        {
            return s_globalHotKeyID++;
        }

        private static void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            // Look up the hot key in the dictionary
            if (msg.message == c_WM_HOTKEY && !handled)
            {
                HotKey hotkey;
                if (s_hotkeyLookupTable.TryGetValue((int)msg.wParam, out hotkey))
                {
                    hotkey._callback.Invoke(hotkey);
                    handled = true;
                }
            }
        }
    }
}

