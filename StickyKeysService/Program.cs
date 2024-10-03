using System;
using System.Threading.Tasks;
using Serilog;
using System.Windows.Forms;

namespace StickyKeysAgent
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(System.IO.Path.Combine(AppContext.BaseDirectory, "Logs", "StickyKeysAgent.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the NotifyIcon
            using (NotifyIcon trayIcon = new NotifyIcon())
            {
                trayIcon.Text = "StickyKeysAgent";
                trayIcon.Icon = new System.Drawing.Icon("stickykeys_FKG_icon.ico");

                // Create a context menu with an Exit option
                ContextMenuStrip contextMenu = new ContextMenuStrip();
                ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
                exitItem.Click += (sender, e) =>
                {
                    Application.Exit();
                };
                contextMenu.Items.Add(exitItem);
                trayIcon.ContextMenuStrip = contextMenu;

                trayIcon.Visible = true;

                // Start the worker
                Worker worker = new Worker();
                Task.Run(() => worker.RunAsync());

                // Run the application message loop
                Application.Run();

                // Cleanup when the application exits
                trayIcon.Visible = false;
            }
        }
    }
}
