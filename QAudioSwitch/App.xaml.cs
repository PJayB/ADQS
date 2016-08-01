using System;
using System.Windows;
using System.Windows.Input;
using AudioSwitchCommon;

namespace QAudioSwitch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        KeyMonitor _spaceKey;
        KeyMonitor _laltKey;
        KeyMonitor _lwinKey;
        KeyMonitor _cKey;
        HotKey _activationHotKey;
        SelectMenuWindow _mainWindow;
        Configuration _config;

        public App()
        {
            // Load the configuration file
            _config = Configuration.Load();

            // Load the window
            _mainWindow = new SelectMenuWindow(_config.ExclusionIDs);

            // Register the hot key
            _activationHotKey = new HotKey(Key.Space, HotKey.ModifierFlags.Windows | HotKey.ModifierFlags.Alt, delegate(HotKey hotkey)
            {
                if (_mainWindow.Visibility != Visibility.Visible)
                {
                    _cKey = new KeyMonitor(Key.C);
                    _cKey.KeyDown += _cKey_KeyDown;

                    _mainWindow.ShowDialog();
                }
            });

            _spaceKey = new KeyMonitor(Key.Space);
            _spaceKey.KeyDown += _keyMonitor_KeyDown;
            _laltKey = new KeyMonitor(Key.LeftAlt);
            _laltKey.KeyUp += _keyMonitor_KeyUp;
            _lwinKey = new KeyMonitor(Key.LWin);
            _lwinKey.KeyUp += _keyMonitor_KeyUp;
        }

        private void _cKey_KeyDown(object sender, KeyMonitor.KeyEventArgs e)
        {
            SiblingExecutable.SpawnSibling("QAudioSwitchConfig.exe");
        }

        private void _keyMonitor_KeyUp(object sender, KeyMonitor.KeyEventArgs e)
        {
            UI.ScheduleAction(this.Dispatcher, delegate
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.Close();
                    _cKey.Dispose();
                    _cKey = null;
                }
            });
        }

        private void _keyMonitor_KeyDown(object sender, KeyMonitor.KeyEventArgs e)
        {
            UI.ScheduleAction(this.Dispatcher, delegate
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.SelectNextActive();
                }
            });
        }
    }
}
