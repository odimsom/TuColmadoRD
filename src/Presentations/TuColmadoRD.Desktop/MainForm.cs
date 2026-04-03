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
    private static readonly bool IsProductionMode =
        string.Equals(Environment.GetEnvironmentVariable("RELEASE_TYPE"), "production", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("APP_ENV"), "production", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase);

    private static bool ShowApiActions => !IsProductionMode;

    private Microsoft.Web.WebView2.WinForms.WebView2 _webView = null!;
    private Panel _splashPanel = null!;
    private Panel _quickActionsBar = null!;
    private Panel _headerPanel = null!;
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
            Size = new Size(132, 132),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = this.Icon?.ToBitmap(),
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = "TuColmadoRD",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblSubtitle = new Label
        {
            Text = "Punto de venta, portal local y control operativo en una sola pantalla.",
            ForeColor = Color.FromArgb(191, 219, 254),
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var summaryCard = BuildSummaryCard();
        
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
        _splashPanel.Controls.Add(lblSubtitle);
        _splashPanel.Controls.Add(summaryCard);
        _splashPanel.Controls.Add(_statusLabel);
        _splashPanel.Controls.Add(progressBar);
        _splashPanel.Controls.Add(_actionPanel);

        this.Controls.Add(_splashPanel);

        // Center splash controls
        this.Load += (s, e) =>
        {
            logoBox.Location = new Point((this.ClientSize.Width - logoBox.Width) / 2, 92);
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, logoBox.Bottom + 10);
            lblSubtitle.Location = new Point((this.ClientSize.Width - lblSubtitle.Width) / 2, lblTitle.Bottom + 10);
            summaryCard.Location = new Point((this.ClientSize.Width - summaryCard.Width) / 2, lblSubtitle.Bottom + 26);
            _statusLabel.Location = new Point((this.ClientSize.Width - _statusLabel.Width) / 2, summaryCard.Bottom + 16);
            progressBar.Location = new Point((this.ClientSize.Width - progressBar.Width) / 2, _statusLabel.Bottom + 24);
            _actionPanel.Location = new Point((this.ClientSize.Width - _actionPanel.Width) / 2, progressBar.Bottom + 24);
        };

        // WebView2
        _webView = new Microsoft.Web.WebView2.WinForms.WebView2
        {
            Dock = DockStyle.Fill,
            Visible = false
        };

        _headerPanel = BuildHeaderPanel();
        _quickActionsBar = BuildQuickActionsBar();
        this.Controls.Add(_webView);
        this.Controls.Add(_headerPanel);
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
            _headerPanel.Visible = true;
            return;
        }

        _statusLabel.Text = "No se pudo cargar la pagina principal local.";
        _actionPanel.Visible = true;
    }

    private Panel BuildActionPanel()
    {
        var panel = new Panel
        {
            Size = new Size(760, 104),
            BackColor = Color.FromArgb(20, 30, 52)
        };

        var hintLabel = new Label
        {
            Text = "Si la vista local tarda en cargar, usa una opcion para continuar.",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Left = 14,
            Top = 8
        };

        var openPortalButton = CreateActionButton("Abrir Portal Local", 14, 38, 210, 36);
        openPortalButton.Click += (_, _) => OpenExternalUrl("http://localhost:5100/");

        if (ShowApiActions)
        {
            var openApiButton = CreateActionButton("Abrir API Local", 234, 38, 210, 36);
            openApiButton.Click += (_, _) => OpenExternalUrl("http://localhost:5200/swagger");
            panel.Controls.Add(openApiButton);
        }

        var retryButton = CreateActionButton("Reintentar", 454, 38, 140, 36);
        retryButton.Click += async (_, _) => await ConfigureWebViewAsync();

        panel.Controls.Add(hintLabel);
        panel.Controls.Add(openPortalButton);
        panel.Controls.Add(retryButton);
        return panel;
    }

    private Panel BuildQuickActionsBar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Color.FromArgb(20, 30, 52)
        };

        var title = new Label
        {
            Text = IsProductionMode ? "MODO PRODUCCION" : "MODO TEST",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            AutoSize = true,
            Left = 14,
            Top = 14
        };

        var openPortalButton = CreateActionButton("Portal Local", 110, 9, 132, 28);
        openPortalButton.Click += (_, _) => OpenExternalUrl("http://localhost:5100/");

        if (ShowApiActions)
        {
            var openApiButton = CreateActionButton("API Local", 250, 9, 132, 28);
            openApiButton.Click += (_, _) => OpenExternalUrl("http://localhost:5200/swagger");
            panel.Controls.Add(openApiButton);
        }

        var reloadButton = CreateActionButton("Recargar", ShowApiActions ? 390 : 250, 9, 132, 28);
        reloadButton.Click += (_, _) =>
        {
            if (_webView.CoreWebView2 != null)
            {
                _webView.CoreWebView2.Reload();
                return;
            }

            _ = ConfigureWebViewAsync();
        };

        panel.Controls.Add(title);
        panel.Controls.Add(openPortalButton);
        panel.Controls.Add(reloadButton);
        return panel;
    }

    private Panel BuildHeaderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 88,
            BackColor = Color.FromArgb(17, 24, 39),
            Visible = false
        };

        var logo = new PictureBox
        {
            Size = new Size(56, 56),
            Left = 18,
            Top = 16,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = this.Icon?.ToBitmap(),
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = "TuColmadoRD",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Left = 88,
            Top = 14
        };

        var subtitle = new Label
        {
            Text = "Operacion local con portal, ventas e inventario listos para el cliente.",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Left = 90,
            Top = 48
        };

        var modeBadge = CreateBadge(IsProductionMode ? "PRODUCCION" : "TEST", 430, Color.FromArgb(127, 29, 29));
        var portalBadge = CreateBadge("Portal 5100", 555, Color.FromArgb(30, 41, 59));
        var apiBadge = CreateBadge(ShowApiActions ? "API 5200" : "API oculta", 670, ShowApiActions ? Color.FromArgb(30, 41, 59) : Color.FromArgb(51, 65, 85));

        panel.Controls.Add(logo);
        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(modeBadge);
        panel.Controls.Add(portalBadge);
        panel.Controls.Add(apiBadge);
        return panel;
    }

    private Panel BuildSummaryCard()
    {
        var panel = new Panel
        {
            Size = new Size(760, 112),
            BackColor = Color.FromArgb(17, 24, 39),
            BorderStyle = BorderStyle.FixedSingle
        };

        panel.Controls.Add(CreateMetric("Portal Local", "http://localhost:5100", 26, 18));
        panel.Controls.Add(CreateMetric("API Local", ShowApiActions ? "Disponible en test" : "Oculta en produccion", 280, 18));
        panel.Controls.Add(CreateMetric("Estado", "Servicios iniciando", 534, 18));
        panel.Controls.Add(CreateMetric("Atajos", "Portal, recargar y soporte local", 26, 60));

        return panel;
    }

    private Control CreateMetric(string label, string value, int left, int top)
    {
        var container = new Panel
        {
            Size = new Size(210, 36),
            Left = left,
            Top = top,
            BackColor = Color.Transparent
        };

        container.Controls.Add(new Label
        {
            Text = label,
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 8),
            AutoSize = true,
            Left = 0,
            Top = 0
        });

        container.Controls.Add(new Label
        {
            Text = value,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = 15
        });

        return container;
    }

    private Label CreateBadge(string text, int left, Color backColor)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Left = left,
            Top = 28,
            Padding = new Padding(12, 6, 12, 6),
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private Button CreateActionButton(string text, int left, int top, int width, int height)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = height,
            Left = left,
            Top = top,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(96, 165, 250);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);

        if (!ShowApiActions && text.Contains("API", StringComparison.OrdinalIgnoreCase))
        {
            button.BackColor = Color.FromArgb(51, 65, 85);
            button.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
            button.ForeColor = Color.FromArgb(148, 163, 184);
            button.Cursor = Cursors.No;
        }

        return button;
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
        _headerPanel.Visible = true;
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
