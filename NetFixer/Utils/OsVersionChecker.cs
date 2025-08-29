using System.Runtime.InteropServices;

namespace NetFixer.Utils
{
    public class OsVersionChecker
    {
        public static bool IsWindows7()
        {
            var os = Environment.OSVersion;
            return os.Platform == PlatformID.Win32NT &&
                   os.Version.Major == 6 &&
                   os.Version.Minor == 1;
        }

        public static bool IsMacOS()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
    }
}
