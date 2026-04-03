using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TuColmadoRD.Desktop;

public partial class MainForm : Form
{
    private Microsoft.Web.WebView2.WinForms.WebView2 _webView;
    private Panel _splashPanel;
    private readonly string _startUrl;
    private bool _updateCheckStarted;
    private bool _webViewInitialized;

    public MainForm(string startUrl)
    {
        _startUrl = startUrl;
        InitializeComponent();
        this.Shown += async (_, _) =>
        {
            if (_webViewInitialized)
            {
                return;
            }

            _webViewInitialized = true;
            await ConfigureWebViewAsync();
        };
    }

    private void InitializeComponent()
    {
        this.Text = "TuColmadoRD - Punto de Venta";
        this.Size = new Size(1280, 800);
        this.MinimumSize = new Size(1024, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        var appIconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(appIconPath))
        {
            this.Icon = new Icon(appIconPath);
        }
        else
        {
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        // Splash Panel
        _splashPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 23, 42) // slate-900
        };

        var logoBox = new PictureBox
        {
            Size = new Size(120, 120),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = this.Icon?.ToBitmap(),
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "TuColmadoRD",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };
        
        var lblStatus = new Label
        {
            Text = "Iniciando servicios locales...",
            ForeColor = Color.FromArgb(148, 163, 184), // slate-400
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            Width = 300,
            Height = 4,
            MarqueeAnimationSpeed = 30
        };

        _splashPanel.Controls.Add(logoBox);
        _splashPanel.Controls.Add(lblTitle);
        _splashPanel.Controls.Add(lblStatus);
        _splashPanel.Controls.Add(progressBar);

        this.Controls.Add(_splashPanel);

        // Center splash controls
        this.Load += (s, e) =>
        {
            logoBox.Location = new Point((this.ClientSize.Width - logoBox.Width) / 2, (this.ClientSize.Height - logoBox.Height) / 2 - 130);
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, logoBox.Bottom + 12);
            lblStatus.Location = new Point((this.ClientSize.Width - lblStatus.Width) / 2, lblTitle.Bottom + 10);
            progressBar.Location = new Point((this.ClientSize.Width - progressBar.Width) / 2, lblStatus.Bottom + 30);
        };

        // WebView2
        _webView = new Microsoft.Web.WebView2.WinForms.WebView2
        {
            Dock = DockStyle.Fill,
            Visible = false
        };
        this.Controls.Add(_webView);

        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.F11)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        };
    }

    private async Task ConfigureWebViewAsync()
    {
        try
        {
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TuColmadoRD",
                "WebView2"
            );
            Directory.CreateDirectory(userDataFolder);

            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: userDataFolder,
                options: new CoreWebView2EnvironmentOptions("--disable-features=msWebOOUI,msPdfOOUI")
            );

            await _webView.EnsureCoreWebView2Async(environment);
            
            #if !DEBUG
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            #endif

            _webView.Source = new Uri(_startUrl);
            
            // Wait for first load
            _webView.NavigationCompleted += (s, e) =>
            {
                _splashPanel.Visible = false;
                _webView.Visible = true;

                if (!_updateCheckStarted)
                {
                    _updateCheckStarted = true;
                    _ = CheckForUpdatesAsync();
                }
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al iniciar WebView2: {ex.Message}", "TuColmadoRD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ShowWebView()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(ShowWebView));
            return;
        }
        _splashPanel.Visible = false;
        _webView.Visible = true;
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var result = await UpdateService.CheckForUpdateAsync();
            if (!result.IsUpdateAvailable)
            {
                return;
            }

            var prompt = $"Hay una nueva version disponible: {result.LatestVersion}.\n\nDesea descargar e instalar ahora?";
            var answer = MessageBox.Show(prompt, "Actualizacion disponible", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (answer != DialogResult.Yes)
            {
                return;
            }

            var downloadedInstaller = await UpdateService.DownloadInstallerAsync(result.InstallerUrl!);
            UpdateService.LaunchInstaller(downloadedInstaller);
            Application.Exit();
        }
        catch
        {
            // Do not block POS startup if update check fails.
        }
    }
}
