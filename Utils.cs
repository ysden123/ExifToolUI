using System.IO;
using System.Text;

namespace ExifToolUI
{
    internal static class Utils
    {
        public static string WrtiteArguments(string filePath)
        {
            // Temporary file for arguments
            string argsFile = Path.Combine(
                Path.GetTempPath(),
                $"exifargs_{Guid.NewGuid():N}.txt");

            // Each argument is written on a separate line.
            // UTF-8 without BOM.
            File.WriteAllText(
                argsFile,
                $"{filePath}\n",
                new UTF8Encoding(false));
            return argsFile;
        }
    }
}
