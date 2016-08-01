using Microsoft.Win32;
using System;

namespace ADQSCommon
{
    public static class RunOnStartUp
    {
        const string c_RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        const string c_RunKeyName = "ADQS";

        public static bool IsEnabled
        {
            get
            {
                try
                {
                    var runKey = Registry.CurrentUser.OpenSubKey(c_RunKeyPath);
                    var obj = runKey.GetValue(c_RunKeyName) as string;
                    return obj != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static void Disable()
        {
            try
            {
                var runKey = Registry.CurrentUser.CreateSubKey(c_RunKeyPath);
                runKey.DeleteValue(c_RunKeyName);
            }
            catch (Exception)
            {
            }
        }

        public static void Enable(string startUpCommand)
        {
            try
            {
                var runKey = Registry.CurrentUser.CreateSubKey(c_RunKeyPath);
                runKey.SetValue(c_RunKeyName, startUpCommand);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not modify startup registry key.", ex);
            }
        }
    }
}
