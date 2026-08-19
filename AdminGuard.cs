using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Rakawatch;

internal static class AdminGuard
{
    private const int SW_SHOW = 5;
    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONERROR = 0x00000010;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr ShellExecuteW(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void EnsureAdministrator()
    {
        if (IsAdministrator())
            return;

        var args = Environment.GetCommandLineArgs();
        var trailingArgs = args.Length > 1
            ? string.Join(" ", args.Skip(1).Select(Quote))
            : string.Empty;

        var processPath = Environment.ProcessPath;
        if (processPath is null)
            return;

        var isDotnet = string.Equals(
            Path.GetFileNameWithoutExtension(processPath),
            "dotnet",
            StringComparison.OrdinalIgnoreCase);

        var parameters = isDotnet
            ? $"\"{Assembly.GetEntryAssembly()?.Location}\" {trailingArgs}".Trim()
            : trailingArgs;

        var result = ShellExecuteW(IntPtr.Zero, "runas", processPath, parameters, Environment.CurrentDirectory, SW_SHOW);

        if (result.ToInt64() <= 32)
        {
            MessageBoxW(
                IntPtr.Zero,
                "Rakawatch membutuhkan hak Administrator untuk membaca sensor hardware.\nIzinkan permintaan UAC untuk melanjutkan.",
                "Rakawatch - Perlu Administrator",
                MB_OK | MB_ICONERROR);
        }

        Environment.Exit(result.ToInt64() <= 32 ? 1 : 0);
    }

    private static string Quote(string value) =>
        value.Contains(' ') ? $"\"{value}\"" : value;
}