using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace AudioSwitchCommon
{
    public class SiblingExecutable
    {
        public static string GetSiblingPath(string name)
        {
            // Get the current application's path
            Uri currentAssembly = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

            string currentPath = Path.GetDirectoryName(currentAssembly.LocalPath);
            return Path.Combine(currentPath, name);
        }

        public static Process SpawnSibling(string name)
        {
            string siblingPath = GetSiblingPath(name);

            if (!File.Exists(siblingPath))
            {
                throw new FileNotFoundException($"Couldn't find sibling module '{siblingPath}'");
            }

            Process process = new Process();
            process.StartInfo.FileName = siblingPath;
            if (!process.Start())
            {
                throw new Exception($"Failed to start sibling process '{siblingPath}'");
            }

            return process;
        }

        public static Process FindSiblingProcess(string name)
        {
            name = name.Replace(".exe", "");

            Process[] processes = Process.GetProcessesByName(name);
            return processes.Length > 0 ? processes[0] : null;
        }
    }
}
