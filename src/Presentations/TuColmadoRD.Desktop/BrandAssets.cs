using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace TuColmadoRD.Desktop;

internal static class BrandAssets
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Bitmap CreateLogoBitmap(int size)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        var squareSize = Math.Max(8, (int)(size * 0.42f));
        var gap = Math.Max(2, (int)(size * 0.08f));
        var leftX = Math.Max(1, (size - (squareSize * 2 + gap)) / 2);
        var topY = Math.Max(1, (size - squareSize) / 2);

        using var blueBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
        using var redBrush = new SolidBrush(Color.FromArgb(220, 38, 38));

        var leftSquare = new Rectangle(leftX, topY, squareSize, squareSize);
        var rightSquare = new Rectangle(leftX + squareSize + gap, topY, squareSize, squareSize);

        g.FillRectangle(blueBrush, leftSquare);
        g.FillRectangle(redBrush, rightSquare);

        return bmp;
    }

    public static Icon CreateLogoIcon(int size)
    {
        using var bitmap = CreateLogoBitmap(size);
        var handle = bitmap.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(handle).Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

}