using Microsoft.Web.WebView2.WinForms;

namespace TuColmadoRD.Desktop;

public partial class MainForm : Form
{
    private WebView2 _webView;
    private Panel _splashPanel;
    private readonly string _startUrl;

    public MainForm(string startUrl)
    {
        _startUrl = startUrl;
        InitializeComponent();
        ConfigureWebView();
    }

    private void InitializeComponent()
    {
        this.Text = "TuColmadoRD - Punto de Venta";
        this.Size = new Size(1280, 800);
        this.MinimumSize = new Size(1024, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        // Splash Panel
        _splashPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 23, 42), // slate-900
            ZOrder = 1
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

        _splashPanel.Controls.Add(lblTitle);
        _splashPanel.Controls.Add(lblStatus);
        _splashPanel.Controls.Add(progressBar);

        this.Controls.Add(_splashPanel);

        // Center splash controls
        this.Load += (s, e) =>
        {
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, (this.ClientSize.Height - lblTitle.Height) / 2 - 40);
            lblStatus.Location = new Point((this.ClientSize.Width - lblStatus.Width) / 2, lblTitle.Bottom + 10);
            progressBar.Location = new Point((this.ClientSize.Width - progressBar.Width) / 2, lblStatus.Bottom + 30);
        };

        // WebView2
        _webView = new WebView2
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

    private async void ConfigureWebView()
    {
        try
        {
            await _webView.EnsureCoreWebView2Async(null);
            
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
}
