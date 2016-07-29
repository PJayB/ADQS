using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace QAudioSwitch
{
    static class IconResourceCache
    {
        struct IconHandle
        {
            public IntPtr Large;
            public IntPtr Small;
        }

        static Dictionary<string, IconHandle> sCache = new Dictionary<string, IconHandle>();

        [DllImport("Shell32.dll", EntryPoint="ExtractIconExW", CharSet=CharSet.Unicode, ExactSpelling=true, CallingConvention=CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        class IconReference
        {
            public string File;
            public int Ordinal;

            public IconReference()
            {
            }

            public IconReference(string file, int ordinal)
            {
                File = file;
                Ordinal = ordinal;
            }

            public IconReference(string fileAndOrdinal)
            {
                // Separate the file and ordinal
                int indexOfComma = fileAndOrdinal.LastIndexOf(',');
                if (indexOfComma == -1)
                    throw new Exception("File and ordinal string not valid. Format should be file.dll,index.");

                string file = fileAndOrdinal.Substring(0, indexOfComma);

                // Expand environment variables
                File = Environment.ExpandEnvironmentVariables(file);

                string ordinalString = fileAndOrdinal.Substring(indexOfComma + 1);
                if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(ordinalString))
                    throw new Exception("Either file or ordinal string not valid.");

                if (!int.TryParse(ordinalString, out Ordinal))
                    throw new Exception("Ordinal provided isn't a valid integer.");
            }            

            public override string ToString()
            {
                return File + "," + Ordinal;
            }
        }

        private static IntPtr LoadCachedIconHandle(IconReference icon, bool large)
        {
            string fileAndOrdinal = icon.ToString();
            if (sCache.ContainsKey(fileAndOrdinal))
            {
                return large ? sCache[fileAndOrdinal].Large : sCache[fileAndOrdinal].Small;
            }

            IconHandle handle;
            int extracted = ExtractIconEx(icon.File, icon.Ordinal, out handle.Large, out handle.Small, 1);
            if (extracted < 1)
            {
                // Failed
                throw new Exception($"Couldn't load icon {fileAndOrdinal}.");
            }

            sCache.Add(fileAndOrdinal, handle);

            return large ? handle.Large : handle.Small;
        }

        public static Icon LoadIcon(string file, int ordinal, bool large)
        {
            return Icon.FromHandle(LoadCachedIconHandle(new IconReference(file, ordinal), large));
        }

        public static Icon LoadIcon(string fileAndOrdinal, bool large)
        {
            return Icon.FromHandle(LoadCachedIconHandle(new IconReference(fileAndOrdinal), large));
        }

        public static BitmapSource LoadIconAsBitmap(string file, int ordinal, bool large)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                LoadCachedIconHandle(new IconReference(file, ordinal), large),
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource LoadIconAsBitmap(string fileAndOrdinal, bool large)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                LoadCachedIconHandle(new IconReference(fileAndOrdinal), large),
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
