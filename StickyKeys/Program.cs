using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StickyKeys;

internal static class Program
{
    const uint SKF_AVAILABLE = 0x00000002;
    const uint SKF_CONFIRMHOTKEY = 0x00000008;
    const uint SKF_STICKYKEYSON = 0x00000001;
    const uint SKF_HOTKEYACTIVE = 0x00000004;
    const uint SKF_INDICATOR = 0x00000020;
    const uint SKF_TRISTATE = 0x00000080;
    const uint SKF_TWOKEYSOFF = 0x00000100;
    const uint SPI_SETSTICKYKEYS = 0x003B;
    const uint SPIF_SENDCHANGE = 0x2;

    const string userKey = @"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys";
    const string flagsValue = "Flags";

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref Stickykeys pvParam, uint fWinIni);

    [StructLayout(LayoutKind.Sequential)]
    private struct Stickykeys
    {
        public uint cbSize;
        public uint dwFlags;
    }

    private static void Main(string[] args)
    {
        var stickyKeys = new Stickykeys();
        stickyKeys.cbSize = (uint)Marshal.SizeOf(stickyKeys);
        stickyKeys.dwFlags = SKF_AVAILABLE | SKF_CONFIRMHOTKEY | SKF_STICKYKEYSON | SKF_HOTKEYACTIVE | SKF_INDICATOR | SKF_TRISTATE | SKF_TWOKEYSOFF;
            
        var result = SystemParametersInfo(SPI_SETSTICKYKEYS, (uint)Marshal.SizeOf(stickyKeys), ref stickyKeys, SPIF_SENDCHANGE);
        Console.WriteLine(result
            ? "Successfully set sticky keys settings."
            : "Failed to set sticky keys settings.");

        if (!result) return;
        try
        {
            // Backup existing settings
            var originalFlags = Registry.GetValue(userKey, flagsValue, null)?.ToString();
            File.WriteAllText("StickyKeysBackup.txt", originalFlags ?? "null");

            // Save new settings to registry
            SaveStickyKeysToRegistry();
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Access to the registry is denied. Please run this program as an administrator.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating the registry: {ex.Message}");
        }
    }

    private static void SaveStickyKeysToRegistry()
    {
        // Convert dwFlags to a string
        var flags = (SKF_AVAILABLE | SKF_CONFIRMHOTKEY | SKF_STICKYKEYSON | SKF_HOTKEYACTIVE | SKF_INDICATOR | SKF_TRISTATE | SKF_TWOKEYSOFF).ToString();

        Registry.SetValue(userKey, flagsValue, flags, RegistryValueKind.String);
    }
}