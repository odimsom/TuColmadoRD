using System.Drawing;
using System.Windows.Forms;

namespace TuColmadoRD.Desktop;

internal sealed class SplashForm : Form
{
    private readonly Label _statusLabel;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(400, 220);
        BackColor = AppTheme.Background;
        ShowInTaskbar = false;
        TopMost = true;

        var logo = new PictureBox
        {
            Size = new Size(64, 64),
            Location = new Point((Width - 64) / 2, 36),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = SystemIcons.Application.ToBitmap(),
            BackColor = Color.Transparent
        };

        var title = new Label
        {
            Text = "Iniciando TuColmadoRD...",
            ForeColor = AppTheme.TextMuted,
            Font = new Font("Segoe UI", 13, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(108, 116)
        };

        var progress = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 28,
            Width = 280,
            Height = 4,
            Location = new Point(60, 156)
        };

        _statusLabel = new Label
        {
            Text = "Preparando servicios locales...",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(114, 172)
        };

        Controls.Add(logo);
        Controls.Add(title);
        Controls.Add(progress);
        Controls.Add(_statusLabel);
    }

    public void SetStatus(string text)
    {
        _statusLabel.Text = text;
    }
}
