using System;
using System.Threading.Tasks;
using Serilog;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace StickyKeysAgent
{
    public class Program
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "StickyKeysAgent_.log"), rollingInterval: RollingInterval.Month)
                .CreateLogger();

            // Build configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .Build();

            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddTransient<Worker>();
            var serviceProvider = services.BuildServiceProvider();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load configuration
            var settings = configuration.Get<ConfigSettings>();

            // Check if it's the first run
            if (settings.FirstRun)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to start this application with Windows?",
                    "First Run Setup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                settings.Autostart = result == DialogResult.Yes;
                settings.FirstRun = false;

                configuration["Autostart"] = settings.Autostart.ToString();
                configuration["FirstRun"] = settings.FirstRun.ToString();
                SaveConfigSettings(configuration);
                SaveAutostartSetting(settings.Autostart);
            }

            // Initialize the NotifyIcon
            using (NotifyIcon trayIcon = new NotifyIcon())
            {
                trayIcon.Text = "StickyKeysAgent";
                trayIcon.Icon = new System.Drawing.Icon("stickykeys_FKG_icon.ico");

                // Create a context menu with an Exit option and Autostart option
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                // New Autostart option
                ToolStripMenuItem autostartItem = new ToolStripMenuItem("Autostart with Windows")
                {
                    Checked = settings.Autostart // Load from config
                };
                autostartItem.Click += (sender, e) =>
                {
                    autostartItem.Checked = !autostartItem.Checked; // Toggle checked state
                    settings.Autostart = autostartItem.Checked; // Update config
                    SaveConfigSettings(configuration); // Save the new setting to config
                    SaveAutostartSetting(settings.Autostart); // Save to registry
                };
                contextMenu.Items.Add(autostartItem);

                ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
                exitItem.Click += (sender, e) => Application.Exit();
                contextMenu.Items.Add(exitItem);

                trayIcon.ContextMenuStrip = contextMenu;

                trayIcon.Visible = true;

                // Start the worker
                var worker = serviceProvider.GetRequiredService<Worker>();
                Task.Run(() => worker.RunAsync());

                // Run the application message loop
                Application.Run();

                // Cleanup when the application exits
                trayIcon.Visible = false;
            }
        }

        // Method to save configuration settings to config.json
        static void SaveConfigSettings(IConfiguration configuration)
        {

            var configFile = Path.Combine(AppContext.BaseDirectory, "config.json");
            var json = JsonSerializer.Serialize(configuration.Get<ConfigSettings>(), _jsonOptions);
            File.WriteAllText(configFile, json);

            // Reload the configuration to reflect changes
            ((IConfigurationRoot)configuration).Reload();
        }

        // Method to save autostart setting
        static void SaveAutostartSetting(bool enable)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    if (key?.GetValue("StickyKeysAgent") == null)
                    {
                        key.SetValue("StickyKeysAgent", Application.ExecutablePath);
                    }
                }
                else
                {
                    key?.DeleteValue("StickyKeysAgent", false);
                }
            }
        }
    }
}
