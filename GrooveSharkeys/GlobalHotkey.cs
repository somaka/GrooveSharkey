using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GrooveSharkeys
{
    public class GlobalHotkey : IDisposable
    {
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32", SetLastError = true)]
        public static extern int UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;

        public const int WM_HOTKEY = 0x312;

        public GlobalHotkey()
        {
            Handle = Process.GetCurrentProcess().MainWindowHandle;
        }

        /// <summary> handle of the current process </summary>
        public IntPtr Handle;

        /// <summary> the ID for the hotkey </summary>
        public short HotkeyID { get; private set; }

        /// <summary> register the hotkey </summary>
        public void RegisterGlobalHotKey(Keys keys, IntPtr handle)
        {
            var modifiers = 0;
            var mods = new Dictionary<Keys, int>
                           {
                               {Keys.Alt, MOD_ALT},
                               {Keys.Control, MOD_CONTROL},
                               {Keys.Shift, MOD_SHIFT},
                           };

            foreach (var key in mods.Keys
                        .Where(key => (keys & key) != 0))
            {
                modifiers |= mods[key];
                keys &= ~key;
            }

            RegisterGlobalHotKey((int)keys, modifiers, handle);
        }

        /// <summary> register the hotkey </summary>
        public void RegisterGlobalHotKey(int hotkey, int modifiers, IntPtr handle)
        {
            UnregisterGlobalHotKey();
            Handle = handle;
            RegisterGlobalHotKey(hotkey, modifiers);
        }

        /// <summary> register the hotkey </summary>
        public void RegisterGlobalHotKey(int hotkey, int modifiers)
        {
            UnregisterGlobalHotKey();

            try
            {
                // use the GlobalAddAtom API to get a unique ID (as suggested by MSDN docs)
                string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + GetHashCode();
                HotkeyID = GlobalAddAtom(atomName);
                if (HotkeyID == 0)
                    throw new Exception("Unable to generate unique hotkey ID. Error: " +
                                        Marshal.GetLastWin32Error().ToString());

                // register the hotkey, throw if any error
                if (!RegisterHotKey(this.Handle, HotkeyID, modifiers, (int) hotkey))
                    throw new Exception("Unable to register hotkey. Error: " + Marshal.GetLastWin32Error().ToString());
            }
            catch (Exception e)
            {
                // clean up if hotkey registration failed
                UnregisterGlobalHotKey();
                Console.WriteLine(e);
            }
        }

        /// <summary> unregister the hotkey </summary>
        public void UnregisterGlobalHotKey()
        {
            if (HotkeyID != 0)
            {
                UnregisterHotKey(Handle, HotkeyID);
                // clean up the atom list
                GlobalDeleteAtom(HotkeyID);
                HotkeyID = 0;
            }
        }

        public void Dispose()
        {
            UnregisterGlobalHotKey();
        }
    }
}