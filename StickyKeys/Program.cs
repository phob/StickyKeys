using System.Runtime.InteropServices;

namespace StickyKeys
{
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

        [DllImport("user32.dll")]
        //private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref Stickykeys pvParam, uint fWinIni);
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
        }
    }
}