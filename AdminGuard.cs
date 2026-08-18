using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Rakawatch;

internal static class AdminGuard
{
    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONERROR = 0x00000010;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void NotifyAndExit()
    {
        MessageBoxW(
            IntPtr.Zero,
            "Rakawatch membutuhkan hak Administrator untuk membaca sensor hardware.\nJalankan ulang aplikasi sebagai Administrator.",
            "Rakawatch - Perlu Administrator",
            MB_OK | MB_ICONERROR);

        Environment.Exit(1);
    }
}