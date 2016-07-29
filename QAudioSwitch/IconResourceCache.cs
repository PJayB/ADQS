using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace QAudioSwitch
{
    static class IconResourceCache
    {
        [DllImport("Shell32.dll", EntryPoint="ExtractIconExW", CharSet=CharSet.Unicode, ExactSpelling=true, CallingConvention=CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        struct IconReference
        {
            public string File;
            public int Ordinal;
        }

        private static IntPtr GetIconHandle(string file, int ordinal, bool large)
        {
            IntPtr hLargeIcon, hSmallIcon;
            int extracted = ExtractIconEx(file, ordinal, out hLargeIcon, out hSmallIcon, 1);
            if (extracted < 1)
            {
                // Failed
                throw new Exception($"Couldn't load icon {file},{ordinal}.");
            }

            return large ? hLargeIcon : hSmallIcon;
        }

        private static IconReference GetIconReferenceFromString(string fileAndOrdinal)
        {
            // Separate the file and ordinal
            int indexOfComma = fileAndOrdinal.LastIndexOf(',');
            if (indexOfComma == -1)
                throw new Exception("File and ordinal string not valid. Format should be file.dll,index.");

            string file = fileAndOrdinal.Substring(0, indexOfComma);

            // Expand environment variables
            file = Environment.ExpandEnvironmentVariables(file);

            string ordinalString = fileAndOrdinal.Substring(indexOfComma + 1);
            if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(ordinalString))
                throw new Exception("Either file or ordinal string not valid.");

            int ordinal;
            if (!int.TryParse(ordinalString, out ordinal))
                throw new Exception("Ordinal provided isn't a valid integer.");

            return new IconReference()
            {
                File = file,
                Ordinal = ordinal
            };
        }

        public static Icon LoadIcon(string file, int ordinal, bool large)
        {
            return Icon.FromHandle(GetIconHandle(file, ordinal, large));
        }

        public static Icon LoadIcon(string fileAndOrdinal, bool large)
        {
            var reference = GetIconReferenceFromString(fileAndOrdinal);
            return LoadIcon(reference.File, reference.Ordinal, large);
        }

        public static BitmapSource LoadIconAsBitmap(string file, int ordinal, bool large)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                GetIconHandle(file, ordinal, large),
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource LoadIconAsBitmap(string fileAndOrdinal, bool large)
        {
            var reference = GetIconReferenceFromString(fileAndOrdinal);
            return LoadIconAsBitmap(reference.File, reference.Ordinal, large);
        }
    }
}
