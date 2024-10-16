using System;
using System.Threading.Tasks;
using Serilog;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Diagnostics;
using System.Drawing;

namespace StickyKeysAgent
{
    public class Program
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        private static IConfiguration _configuration;
        private static ConfigSettings _settings;

        [STAThread]
        static void Main()
        {
            ConfigureLogger();
            BuildConfiguration();
            var serviceProvider = ConfigureServices();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _settings = _configuration.Get<ConfigSettings>();
            HandleFirstRun();

            RunApplication(serviceProvider);
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "StickyKeysAgent_.log"), rollingInterval: RollingInterval.Month)
                .CreateLogger();
        }

        private static void BuildConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_configuration);
            services.AddTransient<Worker>();
            return services.BuildServiceProvider();
        }

        private static void HandleFirstRun()
        {
            if (_settings.FirstRun)
            {
                _settings.Autostart = ShowFirstRunDialog();
                _settings.FirstRun = false;
                SaveSettings();
            }
        }

        private static bool ShowFirstRunDialog()
        {
            return MessageBox.Show(
                "Do you want to start this application with Windows?",
                "First Run Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private static void RunApplication(ServiceProvider serviceProvider)
        {
            using (var trayIcon = CreateTrayIcon())
            {
                trayIcon.Visible = true;
                StartWorker(serviceProvider);
                Application.Run();
                trayIcon.Visible = false;
            }
        }

        private static NotifyIcon CreateTrayIcon()
        {
            var trayIcon = new NotifyIcon
            {
                Text = "StickyKeysAgent",
                Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
            return trayIcon;
        }

        private static ContextMenuStrip CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(CreateAutostartMenuItem());
            contextMenu.Items.Add(CreateExitMenuItem());
            return contextMenu;
        }

        private static ToolStripMenuItem CreateAutostartMenuItem()
        {
            var autostartItem = new ToolStripMenuItem("Autostart with Windows")
            {
                Checked = _settings.Autostart
            };
            autostartItem.Click += (sender, e) => ToggleAutostart(autostartItem);
            return autostartItem;
        }

        private static ToolStripMenuItem CreateExitMenuItem()
        {
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (sender, e) => Application.Exit();
            return exitItem;
        }

        private static void ToggleAutostart(ToolStripMenuItem autostartItem)
        {
            autostartItem.Checked = !autostartItem.Checked;
            _settings.Autostart = autostartItem.Checked;
            SaveSettings();
            SaveAutostartSetting(_settings.Autostart);
        }

        private static void StartWorker(ServiceProvider serviceProvider)
        {
            var worker = serviceProvider.GetRequiredService<Worker>();
            Task.Run(() => worker.RunAsync());
        }

        private static void SaveSettings()
        {
            var configFile = Path.Combine(AppContext.BaseDirectory, "config.json");
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            File.WriteAllText(configFile, json);
            ((IConfigurationRoot)_configuration).Reload();
        }

        private static void SaveAutostartSetting(bool enable)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    key?.SetValue("StickyKeysAgent", Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    key?.DeleteValue("StickyKeysAgent", false);
                }
            }
        }
    }
}
