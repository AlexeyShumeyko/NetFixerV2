using System.Security.Principal;

namespace NetFixer.Utils
{
    public static class AdminChecker
    {
        public static bool IsRunAsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();

            return new WindowsPrincipal(identity)
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
