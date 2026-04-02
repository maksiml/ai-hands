using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AiHands.Automation;

/// <summary>
/// Represents metadata about a desktop window including its position and size.
/// </summary>
public record WindowInfo(string Handle, string Title, string ClassName, int X, int Y, int Width, int Height);

/// <summary>
/// Manages desktop windows: finding, focusing, moving, resizing, and waiting for windows.
/// </summary>
public static class WindowManager
{
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    private const int SW_MINIMIZE = 6;
    private const int SW_MAXIMIZE = 3;
    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;

    /// <summary>
    /// Enumerates visible windows, optionally filtering by title pattern.
    /// </summary>
    public static List<WindowInfo> FindWindows(string? titlePattern = null, bool useRegex = false)
    {
        var windows = new List<WindowInfo>();
        EnumWindows((hWnd, _) =>
        {
            if (!IsWindowVisible(hWnd)) return true;

            int len = GetWindowTextLength(hWnd);
            if (len == 0) return true;

            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            var title = sb.ToString();

            if (titlePattern is not null)
            {
                bool match = useRegex
                    ? Regex.IsMatch(title, titlePattern, RegexOptions.IgnoreCase)
                    : title.Contains(titlePattern, StringComparison.OrdinalIgnoreCase);
                if (!match) return true;
            }

            var classSb = new StringBuilder(256);
            GetClassName(hWnd, classSb, classSb.Capacity);

            GetWindowRect(hWnd, out var rect);

            windows.Add(new WindowInfo(
                $"0x{hWnd:X}",
                title,
                classSb.ToString(),
                rect.Left, rect.Top,
                rect.Right - rect.Left, rect.Bottom - rect.Top
            ));
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    /// <summary>
    /// Returns the native handle of the first window matching the given title.
    /// </summary>
    public static IntPtr FindWindowHandle(string title, bool useRegex = false)
    {
        var windows = FindWindows(title, useRegex);
        if (windows.Count == 0)
            throw new InvalidOperationException($"No window found matching '{title}'");
        return new IntPtr(Convert.ToInt64(windows[0].Handle, 16));
    }

    /// <summary>
    /// Brings the specified window to the foreground and gives it focus.
    /// </summary>
    public static void FocusWindow(IntPtr hwnd)
    {
        uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        uint appThread = GetCurrentThreadId();

        if (foreThread != appThread)
        {
            AttachThreadInput(foreThread, appThread, true);
            BringWindowToTop(hwnd);
            ShowWindow(hwnd, SW_SHOW);
            AttachThreadInput(foreThread, appThread, false);
        }
        else
        {
            BringWindowToTop(hwnd);
            ShowWindow(hwnd, SW_SHOW);
        }
        SetForegroundWindow(hwnd);
    }

    /// <summary>
    /// Moves the window to the specified screen coordinates, preserving its size.
    /// </summary>
    public static void MoveWindowTo(IntPtr hwnd, int x, int y)
    {
        GetWindowRect(hwnd, out var rect);
        int w = rect.Right - rect.Left;
        int h = rect.Bottom - rect.Top;
        MoveWindow(hwnd, x, y, w, h, true);
    }

    /// <summary>
    /// Resizes the window to the specified dimensions, preserving its position.
    /// </summary>
    public static void ResizeWindowTo(IntPtr hwnd, int width, int height)
    {
        GetWindowRect(hwnd, out var rect);
        MoveWindow(hwnd, rect.Left, rect.Top, width, height, true);
    }

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    public static void MinimizeWindow(IntPtr hwnd) => ShowWindow(hwnd, SW_MINIMIZE);

    /// <summary>
    /// Maximizes the window.
    /// </summary>
    public static void MaximizeWindow(IntPtr hwnd) => ShowWindow(hwnd, SW_MAXIMIZE);

    /// <summary>
    /// Restores the window from a minimized or maximized state.
    /// </summary>
    public static void RestoreWindow(IntPtr hwnd) => ShowWindow(hwnd, SW_RESTORE);

    /// <summary>
    /// Polls for a window matching the title until it appears or the timeout expires.
    /// </summary>
    public static WindowInfo? WaitForWindow(string title, int timeoutMs, bool useRegex = false)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            var windows = FindWindows(title, useRegex);
            if (windows.Count > 0) return windows[0];
            Thread.Sleep(250);
        }
        return null;
    }

    /// <summary>
    /// Retrieves the title, class name, position, and size of a window.
    /// </summary>
    public static WindowInfo GetWindowInfo(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        GetWindowText(hwnd, sb, sb.Capacity);
        var classSb = new StringBuilder(256);
        GetClassName(hwnd, classSb, classSb.Capacity);
        GetWindowRect(hwnd, out var rect);
        return new WindowInfo(
            $"0x{hwnd:X}",
            sb.ToString(),
            classSb.ToString(),
            rect.Left, rect.Top,
            rect.Right - rect.Left, rect.Bottom - rect.Top
        );
    }
}
