using System;
using System.Windows;
using System.Windows.Input;

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
        HotKey _activationHotKey;
        MainWindow _mainWindow;

        public App()
        {
            // Load the window
            _mainWindow = new QAudioSwitch.MainWindow();

            // Register the hot key
            _activationHotKey = new HotKey(Key.Space, HotKey.ModifierFlags.Windows | HotKey.ModifierFlags.Alt, delegate(HotKey hotkey)
            {
                if (_mainWindow.Visibility != Visibility.Visible)
                {
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

        private void _keyMonitor_KeyUp(object sender, KeyMonitor.KeyEventArgs e)
        {
            Utils.ScheduleUIAction(this.Dispatcher, delegate
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.Close();
                }
            });
        }

        private void _keyMonitor_KeyDown(object sender, KeyMonitor.KeyEventArgs e)
        {
            Utils.ScheduleUIAction(this.Dispatcher, delegate
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.SelectNextActive();
                }
            });
        }
    }
}
