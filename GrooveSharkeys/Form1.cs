using System;
using System.Windows.Forms;

namespace GrooveSharkeys
{
    public partial class MainForm : Form
    {
        private readonly GlobalHotkey _playPauseHotKey = new GlobalHotkey();
        private readonly GlobalHotkey _stopHotKey = new GlobalHotkey();
        private readonly GlobalHotkey _nextHotKey = new GlobalHotkey();
        private readonly GlobalHotkey _prevHotKey = new GlobalHotkey();
        
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            _playPauseHotKey.RegisterGlobalHotKey(Keys.Up | Keys.Control | Keys.Alt, Handle);
            _stopHotKey.RegisterGlobalHotKey(Keys.Down | Keys.Control | Keys.Alt, Handle);
            _nextHotKey.RegisterGlobalHotKey(Keys.Right | Keys.Control | Keys.Alt, Handle);
            _prevHotKey.RegisterGlobalHotKey(Keys.Left | Keys.Control | Keys.Alt, Handle);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _playPauseHotKey.Dispose();
            _stopHotKey.Dispose();
            _prevHotKey.Dispose();
            _nextHotKey.Dispose();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg != GlobalHotkey.WM_HOTKEY)
            {
                base.WndProc(ref m);
                return;
            }
            
            if ((short) m.WParam == _playPauseHotKey.HotkeyID)
            {
                InvokeFlashMethod("togglePlayback");
            }
            else if ((short) m.WParam == _stopHotKey.HotkeyID)
            {
                InvokeFlashMethod("pausePlayback");
            }
            else if ((short)m.WParam == _prevHotKey.HotkeyID)
            {
                InvokeFlashMethod("previous");
            }
            else if ((short)m.WParam == _nextHotKey.HotkeyID)
            {
                InvokeFlashMethod("next");
            }
        }

        private void InvokeFlashMethod(string method)
        {
            webBrowser1.Document
                .GetElementById("gsliteswf")
                .InvokeMember("CallFunction", 
                    @"<invoke name="""+ method + @"""></invoke>");
        }
    }
}
