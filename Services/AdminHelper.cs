using System.Security.Principal;

namespace ShutUp11.Services
{
    public static class AdminHelper
    {
        public static bool IsRunningAsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}