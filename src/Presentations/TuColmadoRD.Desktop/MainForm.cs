using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TuColmadoRD.Desktop;

public partial class MainForm : Form
{
    private Microsoft.Web.WebView2.WinForms.WebView2 _webView = null!;
    private Panel _splashPanel = null!;
    private Panel _quickActionsBar = null!;
    private Label _statusLabel = null!;
    private Panel _actionPanel = null!;
    private readonly string _startUrl;
    private bool _updateCheckStarted;
    private bool _webViewInitialized;
    private bool _navigationCompletedSuccessfully;

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

            if (!_updateCheckStarted)
            {
                _updateCheckStarted = true;
                _ = CheckForUpdatesAsync();
            }
        };
    }

    private void InitializeComponent()
    {
        this.Text = "TuColmadoRD - Punto de Venta";
        this.Size = new Size(1280, 800);
        this.MinimumSize = new Size(1024, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        var localIconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        if (File.Exists(localIconPath))
        {
            this.Icon = new Icon(localIconPath);
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
        
        _statusLabel = new Label
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

        _actionPanel = BuildActionPanel();
        _actionPanel.Visible = false;

        _splashPanel.Controls.Add(logoBox);
        _splashPanel.Controls.Add(lblTitle);
        _splashPanel.Controls.Add(_statusLabel);
        _splashPanel.Controls.Add(progressBar);
        _splashPanel.Controls.Add(_actionPanel);

        this.Controls.Add(_splashPanel);

        // Center splash controls
        this.Load += (s, e) =>
        {
            logoBox.Location = new Point((this.ClientSize.Width - logoBox.Width) / 2, (this.ClientSize.Height - logoBox.Height) / 2 - 130);
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, logoBox.Bottom + 12);
            _statusLabel.Location = new Point((this.ClientSize.Width - _statusLabel.Width) / 2, lblTitle.Bottom + 10);
            progressBar.Location = new Point((this.ClientSize.Width - progressBar.Width) / 2, _statusLabel.Bottom + 30);
            _actionPanel.Location = new Point((this.ClientSize.Width - _actionPanel.Width) / 2, progressBar.Bottom + 24);
        };

        // WebView2
        _webView = new Microsoft.Web.WebView2.WinForms.WebView2
        {
            Dock = DockStyle.Fill,
            Visible = false
        };

        _quickActionsBar = BuildQuickActionsBar();
        this.Controls.Add(_webView);
        this.Controls.Add(_quickActionsBar);

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
            _statusLabel.Text = "Iniciando servicios locales...";
            _actionPanel.Visible = false;
            _navigationCompletedSuccessfully = false;

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

            _webView.NavigationCompleted -= OnNavigationCompleted;
            _webView.NavigationCompleted += OnNavigationCompleted;
            _webView.Source = new Uri(_startUrl);

            _ = Task.Run(async () =>
            {
                await Task.Delay(15000);
                if (!_navigationCompletedSuccessfully && !IsDisposed)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _statusLabel.Text = "La vista local no respondio a tiempo. Use las opciones para continuar.";
                        _actionPanel.Visible = true;
                    }));
                }
            });
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error al iniciar WebView2: {ex.Message}";
            _actionPanel.Visible = true;
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            _navigationCompletedSuccessfully = true;
            _splashPanel.Visible = false;
            _webView.Visible = true;
            return;
        }

        _statusLabel.Text = "No se pudo cargar la pagina principal local.";
        _actionPanel.Visible = true;
    }

    private Panel BuildActionPanel()
    {
        var panel = new Panel
        {
            Size = new Size(560, 84),
            BackColor = Color.FromArgb(20, 30, 52)
        };

        var openPortalButton = new Button
        {
            Text = "Abrir Portal Local",
            Width = 170,
            Height = 34,
            Left = 14,
            Top = 24
        };
        openPortalButton.Click += (_, _) => OpenExternalUrl("http://localhost:5100/");

        var openApiButton = new Button
        {
            Text = "Abrir API Local",
            Width = 170,
            Height = 34,
            Left = 195,
            Top = 24
        };
        openApiButton.Click += (_, _) => OpenExternalUrl("http://localhost:5200/swagger");

        var retryButton = new Button
        {
            Text = "Reintentar",
            Width = 170,
            Height = 34,
            Left = 376,
            Top = 24
        };
        retryButton.Click += async (_, _) => await ConfigureWebViewAsync();

        panel.Controls.Add(openPortalButton);
        panel.Controls.Add(openApiButton);
        panel.Controls.Add(retryButton);
        return panel;
    }

    private Panel BuildQuickActionsBar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 42,
            BackColor = Color.FromArgb(20, 30, 52)
        };

        var openPortalButton = new Button
        {
            Text = "Portal Local",
            Width = 120,
            Height = 28,
            Left = 10,
            Top = 7
        };
        openPortalButton.Click += (_, _) => OpenExternalUrl("http://localhost:5100/");

        var openApiButton = new Button
        {
            Text = "API Local",
            Width = 120,
            Height = 28,
            Left = 140,
            Top = 7
        };
        openApiButton.Click += (_, _) => OpenExternalUrl("http://localhost:5200/swagger");

        var reloadButton = new Button
        {
            Text = "Recargar",
            Width = 120,
            Height = 28,
            Left = 270,
            Top = 7
        };
        reloadButton.Click += (_, _) =>
        {
            if (_webView.CoreWebView2 != null)
            {
                _webView.CoreWebView2.Reload();
                return;
            }

            _ = ConfigureWebViewAsync();
        };

        panel.Controls.Add(openPortalButton);
        panel.Controls.Add(openApiButton);
        panel.Controls.Add(reloadButton);
        return panel;
    }

    private static void OpenExternalUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
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
