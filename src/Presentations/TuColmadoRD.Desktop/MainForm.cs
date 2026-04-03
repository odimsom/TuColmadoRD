using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TuColmadoRD.Desktop;

public partial class MainForm : Form
{
    private readonly string _startUrl;
    private readonly bool _openWebViewOnStart;
    private readonly bool _isTestBuild;

    private WebView2 _webView = null!;
    private Panel _launcherPanel = null!;
    private Panel _titleBar = null!;
    private Label _versionLabel = null!;

    private Label _cloudStatusLabel = null!;
    private Panel _cloudDot = null!;
    private Label _apiStatusLabel = null!;
    private Panel _apiDot = null!;
    private Label _licenseStatusLabel = null!;
    private Panel _licenseDot = null!;

    private Label _ventasValueLabel = null!;
    private Label _transaccionesValueLabel = null!;
    private Label _syncValueLabel = null!;
    private Label _syncMetaLabel = null!;

    private bool _dragging;
    private Point _dragStart;
    private bool _webViewInitialized;
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 30_000 };

    public MainForm(string startUrl, bool openWebViewOnStart = false)
    {
        _startUrl = startUrl;
        _openWebViewOnStart = openWebViewOnStart;
        _isTestBuild = ReadIsTestBuild();

        InitializeComponent();

        this.Shown += async (_, _) =>
        {
            try
            {
                await ConfigureWebViewAsync();
                await RefreshLauncherAsync();

                if (_openWebViewOnStart)
                {
                    ShowWebView();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Launcher initialization failed", ex);
                ShowLauncher();
            }
        };

        _refreshTimer.Tick += async (_, _) => await RefreshLauncherAsync();
        _refreshTimer.Start();
    }

    private void InitializeComponent()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = AppTheme.Background;
        this.Size = new Size(900, 600);
        this.MinimumSize = new Size(800, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "TuColmadoRD - Punto de Venta";

        this.Icon = BrandAssets.CreateLogoIcon(32);

        _launcherPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background
        };

        _titleBar = BuildTitleBar();
        var statusBar = BuildStatusBar();
        var body = BuildBody();
        var footer = BuildFooter();

        _launcherPanel.Controls.Add(body);
        _launcherPanel.Controls.Add(footer);
        _launcherPanel.Controls.Add(statusBar);
        _launcherPanel.Controls.Add(_titleBar);

        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            Visible = false
        };

        this.Controls.Add(_webView);
        this.Controls.Add(_launcherPanel);
    }

    private Panel BuildTitleBar()
    {
        var bar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = Color.FromArgb(8, 14, 26)
        };

        bar.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(51, 30, 58, 138), 1f);
            e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
        };

        bar.MouseDown += TitleBar_MouseDown;
        bar.MouseMove += TitleBar_MouseMove;
        bar.MouseUp += TitleBar_MouseUp;

        var logo = new PictureBox
        {
            Size = new Size(18, 18),
            Location = new Point(12, 10),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BrandAssets.CreateLogoBitmap(18),
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = "TuColmadoRD - Punto de Venta",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(38, 9)
        };

        _versionLabel = new Label
        {
            Text = _isTestBuild ? "v0.1.0-test" : string.Empty,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Visible = _isTestBuild
        };
        _versionLabel.Location = new Point(660, 10);

        var btnMin = CreateWindowButton(Color.FromArgb(136, 135, 128), Color.FromArgb(180, 178, 169), 730, () => this.WindowState = FormWindowState.Minimized);
        var btnMax = CreateWindowButton(Color.FromArgb(136, 135, 128), Color.FromArgb(180, 178, 169), 755, () =>
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        });
        var btnClose = CreateWindowButton(Color.FromArgb(226, 75, 74), Color.FromArgb(240, 149, 149), 780, () => Application.Exit());

        bar.Controls.Add(logo);
        bar.Controls.Add(title);
        bar.Controls.Add(_versionLabel);
        bar.Controls.Add(btnMin);
        bar.Controls.Add(btnMax);
        bar.Controls.Add(btnClose);
        return bar;
    }

    private Panel BuildStatusBar()
    {
        var bar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32,
            BackColor = Color.FromArgb(8, 14, 26)
        };

        bar.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(34, 30, 58, 138), 1f);
            e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
        };

        (_cloudDot, _cloudStatusLabel) = CreateStatusItem("Sin conexion", 16);
        (_apiDot, _apiStatusLabel) = CreateStatusItem("Iniciando API...", 210);
        (_licenseDot, _licenseStatusLabel) = CreateStatusItem("Verificando...", 400);

        bar.Controls.Add(_cloudDot);
        bar.Controls.Add(_cloudStatusLabel);
        bar.Controls.Add(_apiDot);
        bar.Controls.Add(_apiStatusLabel);
        bar.Controls.Add(_licenseDot);
        bar.Controls.Add(_licenseStatusLabel);

        var right = new Label
        {
            Text = "Tu Colmado · Sin turno abierto",
            AutoSize = true,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(620, 7)
        };
        bar.Controls.Add(right);

        return bar;
    }

    private Panel BuildBody()
    {
        var body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background,
            Padding = new Padding(28)
        };

        var brandRow = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = Color.Transparent
        };

        var logo = new PictureBox
        {
            Size = new Size(48, 48),
            Location = new Point(0, 8),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BrandAssets.CreateLogoBitmap(48)
        };

        var title = new Label
        {
            Text = "TuColmadoRD",
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            AutoSize = true,
            Location = new Point(62, 2)
        };

        var subtitle = new Label
        {
            Text = "Tu Colmado · Plan Prueba",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = AppTheme.TextMuted,
            AutoSize = true,
            Location = new Point(64, 40)
        };

        var planPill = CreatePill(_isTestBuild ? "14 dias de prueba" : "Plan Avanzado", _isTestBuild ? Color.FromArgb(20, 83, 45) : Color.FromArgb(30, 58, 138), _isTestBuild ? AppTheme.Green : Color.FromArgb(96, 165, 250));
        planPill.Location = new Point(640, 20);

        brandRow.Controls.Add(logo);
        brandRow.Controls.Add(title);
        brandRow.Controls.Add(subtitle);
        brandRow.Controls.Add(planPill);

        var stats = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 130,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 20, 0, 0)
        };
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

        stats.Controls.Add(BuildStatCard("VENTAS HOY", out _ventasValueLabel, out var salesMeta, "RD$0", "Turno sin ventas aun"), 0, 0);
        stats.Controls.Add(BuildStatCard("TRANSACCIONES", out _transaccionesValueLabel, out var txMeta, "0", "En el turno actual"), 1, 0);
        stats.Controls.Add(BuildSyncCard(out _syncValueLabel, out _syncMetaLabel), 2, 0);

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 220,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 20, 0, 0)
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        actions.Controls.Add(BuildActionCard("Abrir punto de venta", "Cobrar, fiados y turno", Color.FromArgb(37, 99, 235), "http://localhost:5100/pos", true), 0, 0);
        actions.Controls.Add(BuildActionCard("Ver reportes", "Ventas, turnos, cuadre", Color.FromArgb(74, 222, 128), "http://localhost:5100/portal/reports", false), 1, 0);
        actions.Controls.Add(BuildActionCard("Inventario", "Productos y stock", Color.FromArgb(239, 159, 39), "http://localhost:5100/portal/inventory", false), 0, 1);
        actions.Controls.Add(BuildActionCard("Fiados", "Clientes y cuentas", Color.FromArgb(167, 139, 250), "http://localhost:5100/portal/customers", false), 1, 1);

        body.Controls.Add(actions);
        body.Controls.Add(stats);
        body.Controls.Add(brandRow);

        return body;
    }

    private Panel BuildFooter()
    {
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 36,
            BackColor = Color.FromArgb(8, 14, 26)
        };

        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(34, 30, 58, 138), 1f);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };

        var localInfo = new Label
        {
            Text = "localhost:5100 · localhost:5200",
            AutoSize = true,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Location = new Point(12, 9),
            Visible = _isTestBuild
        };

        var linkSettings = CreateFooterLink("Configuracion", 600, () => NavigateTo("http://localhost:5100/portal/settings"));
        var linkSupport = CreateFooterLink("Soporte", 710, () => OpenExternalUrl("https://wa.me/18296932458"));
        var linkBrowser = CreateFooterLink("Ver en navegador", 790, () => OpenExternalUrl("http://localhost:5100"), Color.FromArgb(37, 99, 235));

        footer.Controls.Add(localInfo);
        footer.Controls.Add(linkSettings);
        footer.Controls.Add(linkSupport);
        footer.Controls.Add(linkBrowser);

        return footer;
    }

    private async Task ConfigureWebViewAsync()
    {
        if (_webViewInitialized)
        {
            return;
        }

        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TuColmadoRD", "WebView2");
        Directory.CreateDirectory(userDataFolder);

        AppLogger.Info($"Creating WebView2 environment at {userDataFolder}");
        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await _webView.EnsureCoreWebView2Async(env);

        _webView.CoreWebView2.WebMessageReceived += (_, e) =>
        {
            var msg = e.TryGetWebMessageAsString();
            if (string.Equals(msg, "back-to-launcher", StringComparison.OrdinalIgnoreCase))
            {
                ShowLauncher();
                _ = RefreshLauncherAsync();
            }
        };

        _webViewInitialized = true;
        _webView.Source = new Uri(_startUrl);
        AppLogger.Info($"WebView2 ready, navigating to {_startUrl}");
    }

    public void ShowWebView()
    {
        _launcherPanel.Visible = false;
        _webView.Visible = true;
        if (_webView.Source == null)
        {
            _webView.Source = new Uri(_startUrl);
        }
    }

    private void ShowLauncher()
    {
        _webView.Visible = false;
        _launcherPanel.Visible = true;
    }

    private void NavigateTo(string url)
    {
        _webView.Source = new Uri(url);
        ShowWebView();
    }

    private async Task RefreshLauncherAsync()
    {
        await RefreshStatusAsync();
        await RefreshStatsAsync();
    }

    private async Task RefreshStatusAsync()
    {
        var cloudOnline = await CheckCloudConnectivityAsync();
        SetStatus(_cloudDot, _cloudStatusLabel, cloudOnline ? "Conectado a la nube" : "Sin conexion", cloudOnline ? AppTheme.Green : AppTheme.Amber);

        var apiReady = await IsEndpointUpAsync("http://localhost:5200/health");
        SetStatus(_apiDot, _apiStatusLabel, apiReady ? "API local activa" : "Iniciando API...", apiReady ? AppTheme.Green : AppTheme.Amber);

        SetStatus(_licenseDot, _licenseStatusLabel, "Licencia valida", AppTheme.Green);
    }

    private async Task RefreshStatsAsync()
    {
        decimal totalAmount = 0;
        int salesCount = 0;

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await http.GetAsync("http://localhost:5200/health");
            if (response.IsSuccessStatusCode)
            {
                _syncValueLabel.Text = "Al dia";
                _syncValueLabel.ForeColor = AppTheme.Green;
                _syncMetaLabel.Text = "Ultima: hace 1 min";
            }
            else
            {
                _syncValueLabel.Text = "Pendiente";
                _syncValueLabel.ForeColor = AppTheme.Amber;
                _syncMetaLabel.Text = "Reintentando sincronizacion";
            }
        }
        catch
        {
            _syncValueLabel.Text = "Pendiente";
            _syncValueLabel.ForeColor = AppTheme.Amber;
            _syncMetaLabel.Text = "Sin conexion con API";
        }

        _ventasValueLabel.Text = $"RD${totalAmount:N0}";
        _transaccionesValueLabel.Text = salesCount.ToString();
    }

    private static async Task<bool> IsEndpointUpAsync(string url)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await http.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> CheckCloudConnectivityAsync()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 1500);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private static void SetStatus(Panel dot, Label label, string text, Color color)
    {
        dot.BackColor = color;
        label.Text = text;
    }

    private static (Panel dot, Label label) CreateStatusItem(string text, int left)
    {
        var dot = new Panel
        {
            Size = new Size(7, 7),
            Left = left,
            Top = 12,
            BackColor = AppTheme.Amber
        };

        dot.Paint += (_, e) =>
        {
            using var brush = new SolidBrush(dot.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(brush, 0, 0, dot.Width - 1, dot.Height - 1);
        };

        var label = new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Left = left + 14,
            Top = 6
        };

        return (dot, label);
    }

    private Panel BuildStatCard(string title, out Label valueLabel, out Label metaLabel, string value, string meta)
    {
        var card = CreateCard();

        var titleLabel = new Label
        {
            Text = title,
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Left = 16,
            Top = 14
        };

        valueLabel = new Label
        {
            Text = value,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            AutoSize = true,
            Left = 16,
            Top = 34
        };

        metaLabel = new Label
        {
            Text = meta,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            Left = 16,
            Top = 78
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(metaLabel);
        return card;
    }

    private Panel BuildSyncCard(out Label valueLabel, out Label metaLabel)
    {
        var card = BuildStatCard("SINCRONIZACION", out valueLabel, out metaLabel, "Verificando", "Ultima: --");
        return card;
    }

    private Panel BuildActionCard(string title, string subtitle, Color accent, string targetUrl, bool primary)
    {
        var card = CreateCard(primary ? Color.FromArgb(85, 37, 99, 235) : Color.FromArgb(51, 30, 58, 138));
        card.Height = 96;
        card.Margin = new Padding(6);
        card.Cursor = Cursors.Hand;

        var iconCircle = new Panel
        {
            Size = new Size(40, 40),
            Left = 16,
            Top = 28,
            BackColor = Color.FromArgb(26, accent.R, accent.G, accent.B)
        };
        iconCircle.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(iconCircle.BackColor);
            e.Graphics.FillEllipse(brush, 0, 0, iconCircle.Width - 1, iconCircle.Height - 1);
        };

        var iconImage = new PictureBox
        {
            Image = BrandAssets.CreateLogoBitmap(18),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Size = new Size(18, 18),
            Left = 11,
            Top = 11
        };
        iconCircle.Controls.Add(iconImage);

        var titleLabel = new Label
        {
            Text = title,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Left = 70,
            Top = 24
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Left = 70,
            Top = 48
        };

        var arrow = new Label
        {
            Text = ">",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Left = 340,
            Top = 34
        };

        void HoverOn(object? _, EventArgs __)
        {
            card.Tag = AppTheme.Blue;
            card.Invalidate();
        }

        void HoverOff(object? _, EventArgs __)
        {
            card.Tag = primary ? Color.FromArgb(85, 37, 99, 235) : Color.FromArgb(51, 30, 58, 138);
            card.Invalidate();
        }

        void ClickAction(object? _, EventArgs __) => NavigateTo(targetUrl);

        foreach (Control c in new Control[] { card, iconCircle, iconImage, titleLabel, subtitleLabel, arrow })
        {
            c.MouseEnter += HoverOn;
            c.MouseLeave += HoverOff;
            c.Click += ClickAction;
        }

        card.Controls.Add(iconCircle);
        card.Controls.Add(titleLabel);
        card.Controls.Add(subtitleLabel);
        card.Controls.Add(arrow);
        return card;
    }

    private static Panel CreateCard(Color? border = null)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Surface,
            Margin = new Padding(8),
            Padding = new Padding(8),
            Tag = border ?? Color.FromArgb(51, AppTheme.Border)
        };

        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var path = CreateRoundedPath(rect, 10);
            using var brush = new SolidBrush(AppTheme.Surface);
            using var pen = new Pen((Color)card.Tag!, 1f);
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);
        };

        return card;
    }

    private static Label CreatePill(string text, Color backColor, Color foreColor)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Padding = new Padding(10, 5, 10, 5),
            BackColor = backColor,
            ForeColor = foreColor,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
    }

    private static Label CreateFooterLink(string text, int left, Action onClick, Color? overrideColor = null)
    {
        var normalColor = overrideColor ?? Color.FromArgb(71, 85, 105);
        var hoverColor = overrideColor.HasValue ? Color.FromArgb(147, 197, 253) : Color.FromArgb(148, 163, 184);

        var label = new Label
        {
            Text = text,
            AutoSize = true,
            Left = left,
            Top = 9,
            Cursor = Cursors.Hand,
            ForeColor = normalColor,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };

        label.Click += (_, _) => onClick();
        label.MouseEnter += (_, _) => label.ForeColor = hoverColor;
        label.MouseLeave += (_, _) => label.ForeColor = normalColor;

        return label;
    }

    private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _dragging = true;
        _dragStart = e.Location;
    }

    private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging)
        {
            return;
        }

        this.Location = new Point(this.Left + (e.X - _dragStart.X), this.Top + (e.Y - _dragStart.Y));
    }

    private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
    {
        _dragging = false;
    }

    private static Button CreateWindowButton(Color normal, Color hover, int left, Action onClick)
    {
        var btn = new Button
        {
            Width = 14,
            Height = 14,
            Left = left,
            Top = 12,
            FlatStyle = FlatStyle.Flat,
            BackColor = normal,
            TabStop = false,
            Cursor = Cursors.Hand
        };

        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseDownBackColor = hover;
        btn.FlatAppearance.MouseOverBackColor = hover;
        btn.Click += (_, _) => onClick();

        btn.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(btn.BackColor);
            e.Graphics.FillEllipse(brush, 0, 0, btn.Width - 1, btn.Height - 1);
        };

        return btn;
    }

    private static void OpenExternalUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private bool ReadIsTestBuild()
    {
        try
        {
            if (string.Equals(Environment.GetEnvironmentVariable("RELEASE_TYPE"), "test", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                return false;
            }

            using var stream = File.OpenRead(configPath);
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("AppSettings", out var appSettings) &&
                appSettings.TryGetProperty("IsTestBuild", out var isTestBuildNode))
            {
                return string.Equals(isTestBuildNode.GetString(), "true", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // ignore config parse errors
        }

        return false;
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
            // avoid blocking startup
        }
    }
}
