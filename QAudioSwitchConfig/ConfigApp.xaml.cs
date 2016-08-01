using ADQSCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ADQSConfigApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string c_SiblingExeName = SiblingExecutable.BackgroundServiceExecutableName;
#if DEBUG
        const string c_SiblingExeNameDebug = SiblingExecutable.BackgroundServiceExecutableDebugName;
#endif
        const string c_AppID = "ADQSConfig-{505CC275-FFD4-4ACA-BFE8-6CAC31C19586}";

        HotKey _disabledHotKey;
        UniqueInstance _instanceToken;

        public static bool RestartServiceOnExit = true;

        private void Application_Startup(object sender, StartupEventArgs e)
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

            // Find and kill the service
            Process serviceProcess = SiblingExecutable.FindSiblingProcess(c_SiblingExeName);
#if DEBUG
            if (serviceProcess == null)
            {
                serviceProcess = SiblingExecutable.FindSiblingProcess(c_SiblingExeNameDebug);
            }
#endif
            if (serviceProcess != null)
            {
                try
                {
                    serviceProcess.Kill();

                    // Wait for it to close
                    serviceProcess.WaitForExit();
                }
                catch (Exception)
                {
                    MessageBox.Show("The background application could not be terminated. You may have to manually stop and re-launch it before settings become visible.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Disable the hotkey
            try
            {
                _disabledHotKey = new HotKey(Key.Space, HotKey.ModifierFlags.Windows | HotKey.ModifierFlags.Alt, delegate (HotKey hotkey) { });
            }
            catch (Exception)
            {
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Re-enable the hotkey
            if (_disabledHotKey != null)
            {
                _disabledHotKey.Dispose();
                _disabledHotKey = null;
            }

            // Resume the service
            if (RestartServiceOnExit)
            {
                try
                {
                    SiblingExecutable.SpawnSibling(c_SiblingExeName);
                }
                    catch (Exception)
                {
                    MessageBox.Show($"Unable to restart the {SiblingExecutable.BackgroundServiceName} service. You will have to manually restart it.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Release the unique instance
            if (_instanceToken != null)
            {
                _instanceToken.Dispose();
                _instanceToken = null;
            }
        }
    }
}
