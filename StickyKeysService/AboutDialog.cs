using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace StickyKeysAgent
{
    public class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeDialog();
            AddControls();
        }

        private void InitializeDialog()
        {
            Text = "About StickyKeys Agent";
            Size = new Size(650, 340);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void AddControls()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "StickyKeys Agent";
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? version;
            var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "N/A";
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "N/A";

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(610, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var versionLabel = new Label
            {
                Text = $"Version: {version}",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 65),
                Size = new Size(610, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var infoVersionLabel = new Label
            {
                Text = $"Build: {informationalVersion}",
                Font = new Font("Segoe UI", 8),
                Location = new Point(20, 95),
                Size = new Size(610, 40),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter
            };

            var companyLabel = new Label
            {
                Text = $"Company: {company}",
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 150),
                Size = new Size(610, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var copyrightLabel = new Label
            {
                Text = copyright,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 180),
                Size = new Size(610, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(275, 230),
                Size = new Size(100, 30)
            };

            Controls.Add(titleLabel);
            Controls.Add(versionLabel);
            Controls.Add(infoVersionLabel);
            Controls.Add(companyLabel);
            Controls.Add(copyrightLabel);
            Controls.Add(okButton);

            AcceptButton = okButton;
        }
    }
}
