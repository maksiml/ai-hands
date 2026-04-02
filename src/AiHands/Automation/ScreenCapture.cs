using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AiHands.Automation;

/// <summary>
/// Captures screenshots of the full screen, individual windows, or rectangular regions.
/// </summary>
public static class ScreenCapture
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

    /// <summary>
    /// Captures the entire virtual screen (all monitors) and saves it as a PNG.
    /// </summary>
    public static (string File, int Width, int Height) CaptureFullScreen(string outputPath)
    {
        var bounds = System.Windows.Forms.Screen.AllScreens
            .Select(s => s.Bounds)
            .Aggregate(Rectangle.Union);

        using var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        bmp.Save(outputPath, ImageFormat.Png);
        return (Path.GetFullPath(outputPath), bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Captures a single window by its handle and saves it as a PNG.
    /// </summary>
    public static (string File, int Width, int Height) CaptureWindow(IntPtr hwnd, string outputPath)
    {
        // Try DWM extended frame bounds first (accurate for Aero/DWM windows)
        RECT rect;
        int hr = DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out rect, Marshal.SizeOf<RECT>());
        if (hr != 0)
            GetWindowRect(hwnd, out rect);

        int width = rect.Width;
        int height = rect.Height;

        if (width <= 0 || height <= 0)
            throw new InvalidOperationException("Window has zero size — it may be minimized.");

        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);

        // Try PrintWindow first (works for off-screen/occluded windows)
        var hdc = g.GetHdc();
        bool printed = PrintWindow(hwnd, hdc, 2); // PW_RENDERFULLCONTENT = 2
        g.ReleaseHdc(hdc);

        if (!printed)
        {
            // Fallback to screen copy
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
        }

        bmp.Save(outputPath, ImageFormat.Png);
        return (Path.GetFullPath(outputPath), width, height);
    }

    /// <summary>
    /// Captures a rectangular screen region and saves it as a PNG.
    /// </summary>
    public static (string File, int Width, int Height) CaptureRegion(int x, int y, int w, int h, string outputPath)
    {
        using var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(x, y, 0, 0, new Size(w, h));
        bmp.Save(outputPath, ImageFormat.Png);
        return (Path.GetFullPath(outputPath), w, h);
    }
}
