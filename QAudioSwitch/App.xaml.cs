using System.Windows;
using System.Windows.Input;

namespace QAudioSwitch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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

            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            // Unregister the hot key
            _activationHotKey = null;
        }
    }
}
