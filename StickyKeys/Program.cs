using System.Runtime.InteropServices;

namespace StickyKeys
{
    class Program
    {
        const uint SKF_AVAILABLE = 0x00000002;
        const uint SKF_CONFIRMHOTKEY = 0x00000008;
        const uint SKF_STICKYKEYSON = 0x00000001;
        const uint SKF_HOTKEYACTIVE = 0x00000004;
        const uint SKF_INDICATOR = 0x00000020;
        const uint SKF_TRISTATE = 0x00000080;
        const uint SKF_TWOKEYSOFF = 0x00000100;
        const uint SPI_SETSTICKYKEYS = 0x003B;
        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref STICKYKEYS pvParam, uint fWinIni);

        [StructLayout(LayoutKind.Sequential)]
        struct STICKYKEYS
        {
            public uint cbSize;
            public uint dwFlags;
        }

        static void Main(string[] args)
        {
            STICKYKEYS stickyKeys = new STICKYKEYS();
            stickyKeys.cbSize = (uint)Marshal.SizeOf(stickyKeys);
            stickyKeys.dwFlags = SKF_AVAILABLE | SKF_CONFIRMHOTKEY | SKF_STICKYKEYSON | SKF_HOTKEYACTIVE | SKF_INDICATOR | SKF_TRISTATE | SKF_TWOKEYSOFF;

            bool result = SystemParametersInfo(SPI_SETSTICKYKEYS, (uint)Marshal.SizeOf(stickyKeys), ref stickyKeys, 0);
            if (result)
            {
                Console.WriteLine("Successfully set sticky keys settings.");
            }
            else
            {
                Console.WriteLine("Failed to set sticky keys settings.");
            }
        }
    }
}