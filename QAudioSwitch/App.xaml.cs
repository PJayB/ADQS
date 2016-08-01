using System;
using System.Windows;
using System.Windows.Input;
using ADQSCommon;
using System.Diagnostics;

namespace ADQSBackgroundApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string c_AppName = "Audio Device Quick-Switcher";
        const string c_AppID = "ADQSBackgroundApp-{505CC275-FFD4-4ACA-BFE8-6CAC31C19586}";

        KeyMonitor _spaceKey;
        KeyMonitor _laltKey;
        KeyMonitor _lwinKey;
        HotKey _activationHotKey;
        SelectMenuWindow _mainWindow;
        Configuration _config;
        UniqueInstance _instanceToken;

        public App()
        {
            // If the application is already running, abort
            try
            {
                _instanceToken = UniqueInstance.Acquire(c_AppID);
            }
            catch (Exception)
            { 
                Environment.Exit(0);
                return;
            }

            KeyMonitor.Initialize();

            // Load the configuration file
            try
            {
                _config = Configuration.Load();
            }
            catch (Exception)
            {
                MessageBox.Show($"Unable to load {c_AppName} configuration file. Your settings cannot be loaded.", c_AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Load the window
            try
            { 
                _mainWindow = new SelectMenuWindow(_config.ExclusionIDs);

                // Register the hot key
                _activationHotKey = new HotKey(Key.Space, HotKey.ModifierFlags.Windows | HotKey.ModifierFlags.Alt, delegate(HotKey hotkey)
                {
                    if (_mainWindow.Visibility != Visibility.Visible)
                    {
#if DEBUG
                        KeyMonitor cKey = new KeyMonitor(Key.C);
                        KeyMonitor qKey = new KeyMonitor(Key.Q);
                        cKey.KeyDown += _cKey_KeyDown;
                        qKey.KeyDown += _qKey_KeyDown;
#endif

                        _mainWindow.ShowDialog();

#if DEBUG
                        cKey.Dispose();
                        qKey.Dispose();
#endif
                    }
                });

                _spaceKey = new KeyMonitor(Key.Space);
                _spaceKey.KeyDown += _keyMonitor_KeyDown;
                _laltKey = new KeyMonitor(Key.LeftAlt);
                _laltKey.KeyUp += _keyMonitor_KeyUp;
                _lwinKey = new KeyMonitor(Key.LWin);
                _lwinKey.KeyUp += _keyMonitor_KeyUp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There has been a critical failure and the application will now close. ({ex.Message})", c_AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

#if DEBUG
        private void _qKey_KeyDown(object sender, KeyMonitor.KeyEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to quit {c_AppName}?", "Confirm Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        private void _cKey_KeyDown(object sender, KeyMonitor.KeyEventArgs e)
        {
            try
            {
                SiblingExecutable.SpawnSibling(SiblingExecutable.ConfigurationAppExecutableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load the Configuration panel. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
#endif

        private void _keyMonitor_KeyUp(object sender, KeyMonitor.KeyEventArgs e)
        {
            UI.ScheduleAction(this.Dispatcher, delegate
            {
                if (_mainWindow.Visibility == Visibility.Visible)
                {
                    _mainWindow.Close();
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
