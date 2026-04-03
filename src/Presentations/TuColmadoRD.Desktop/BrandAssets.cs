using System.Drawing;
using System.Drawing.Drawing2D;

namespace TuColmadoRD.Desktop;

internal static class BrandAssets
{
    public static Bitmap CreateLogoBitmap(int size)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        var squareSize = Math.Max(8, (int)(size * 0.52f));
        var overlap = Math.Max(3, (int)(size * 0.12f));
        var leftX = Math.Max(1, (size - (squareSize * 2 - overlap)) / 2);
        var topY = Math.Max(1, (size - squareSize) / 2);

        using var blueBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
        using var redBrush = new SolidBrush(Color.FromArgb(220, 38, 38));

        var leftSquare = new Rectangle(leftX, topY, squareSize, squareSize);
        var rightSquare = new Rectangle(leftX + squareSize - overlap, topY, squareSize, squareSize);

        using var leftPath = CreateRoundedPath(leftSquare, Math.Max(2, size / 9));
        using var rightPath = CreateRoundedPath(rightSquare, Math.Max(2, size / 9));

        g.FillPath(blueBrush, leftPath);
        g.FillPath(redBrush, rightPath);

        return bmp;
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
}