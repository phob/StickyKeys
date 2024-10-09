using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace StickyKeysAgent
{
    public class Worker
    {
        private readonly IConfiguration _configuration;
        private ConfigSettings _settings;
        private bool _running = true;
        private static Mutex _mutex;
        private IDisposable _changeToken;

        public Worker(IConfiguration configuration)
        {
            _configuration = configuration;
            const string appName = "StickyKeysAgent";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Log.Warning("Another instance is already running. Exiting.");
                Environment.Exit(0);
            }

            Log.Information("Worker initialized.");

            // Load settings
            LoadSettings();

            // Watch for configuration changes
            WatchConfigurationChanges();
        }

        private void WatchConfigurationChanges()
        {
            _changeToken?.Dispose();
            _changeToken = _configuration.GetReloadToken().RegisterChangeCallback(OnConfigChanged, null);
        }

        private void OnConfigChanged(object state)
        {
            try
            {
                LoadSettings();
                Log.Information("Configuration reloaded.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reload configuration.");
            }
            finally
            {
                // Re-register the change callback
                WatchConfigurationChanges();
            }
        }

        private void LoadSettings()
        {
            try
            {
                _settings = _configuration.Get<ConfigSettings>();
                Log.Information("Configuration loaded successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load configuration. Using last known good configuration.");
            }
        }

        public async Task RunAsync()
        {
            Log.Information("StickyKeysAgent started.");

            while (_running)
            {
                try
                {
                    // Update Sticky Keys settings
                    UpdateStickyKeysSettings();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error updating Sticky Keys settings.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
            }

            Log.Information("StickyKeysAgent stopped.");
        }

        private void UpdateStickyKeysSettings()
        {
            STICKYKEYS currentStickyKeys = new STICKYKEYS();
            currentStickyKeys.cbSize = Marshal.SizeOf<STICKYKEYS>();

            // Get current Sticky Keys settings
            bool success = SystemParametersInfo(SPI_GETSTICKYKEYS, currentStickyKeys.cbSize, ref currentStickyKeys, 0);
            if (!success)
            {
                Log.Error("Failed to get current Sticky Keys settings.");
                return;
            }

            // Build desired dwFlags from configuration
            uint desiredFlags = 0;

            if (_settings.StickyKeysOn)
                desiredFlags |= SKF_STICKYKEYSON;
            if (_settings.HotKeyActive)
                desiredFlags |= SKF_HOTKEYACTIVE;
            if (_settings.ConfirmHotKey)
                desiredFlags |= SKF_CONFIRMHOTKEY;
            if (_settings.HotKeySound)
                desiredFlags |= SKF_HOTKEYSOUND;
            if (_settings.AudibleFeedback)
                desiredFlags |= SKF_AUDIBLEFEEDBACK;
            if (_settings.TriState)
                desiredFlags |= SKF_TRISTATE;
            if (_settings.TwoKeysOff)
                desiredFlags |= SKF_TWOKEYSOFF;
            if (_settings.TaskIndicator)
                desiredFlags |= SKF_INDICATOR;

            // Define the relevant flags mask
            uint relevantFlags = SKF_STICKYKEYSON | SKF_HOTKEYACTIVE | SKF_CONFIRMHOTKEY |
                                 SKF_HOTKEYSOUND | SKF_AUDIBLEFEEDBACK | SKF_TRISTATE |
                                 SKF_TWOKEYSOFF | SKF_INDICATOR;

            // Compare current settings to desired settings using the mask
            if ((currentStickyKeys.dwFlags & relevantFlags) == (desiredFlags & relevantFlags))
            {
                // Settings are already as desired; no action needed
                Log.Debug("Sticky Keys settings are already up-to-date.");
                return;
            }

            // Settings differ; apply new settings
            STICKYKEYS newStickyKeys = new STICKYKEYS();
            newStickyKeys.cbSize = Marshal.SizeOf<STICKYKEYS>();
            newStickyKeys.dwFlags = desiredFlags;

            success = SystemParametersInfo(SPI_SETSTICKYKEYS, newStickyKeys.cbSize, ref newStickyKeys, SPIF_SENDCHANGE);
            if (!success)
            {
                Log.Error("Failed to set new Sticky Keys settings.");
            }
            else
            {
                Log.Information("Sticky Keys settings updated.");
            }
        }

        // P/Invoke declarations
        private const uint SPI_GETSTICKYKEYS = 0x003A;
        private const uint SPI_SETSTICKYKEYS = 0x003B;
        private const uint SPIF_SENDCHANGE = 0x0002;

        private const uint SKF_STICKYKEYSON = 0x00000001;
        private const uint SKF_AVAILABLE = 0x00000002;
        private const uint SKF_HOTKEYACTIVE = 0x00000004;
        private const uint SKF_CONFIRMHOTKEY = 0x00000008;
        private const uint SKF_HOTKEYSOUND = 0x00000010;
        private const uint SKF_INDICATOR = 0x00000020;
        private const uint SKF_AUDIBLEFEEDBACK = 0x00000040;
        private const uint SKF_TRISTATE = 0x00000080;
        private const uint SKF_TWOKEYSOFF = 0x00000100;

        [StructLayout(LayoutKind.Sequential)]
        struct STICKYKEYS
        {
            public int cbSize;
            public uint dwFlags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SystemParametersInfo(uint uiAction, int uiParam, ref STICKYKEYS pvParam, uint fWinIni);
    }
}
