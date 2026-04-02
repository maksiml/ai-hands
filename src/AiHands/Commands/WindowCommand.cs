using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "window" CLI command for finding, focusing, moving, resizing, and managing windows.
/// </summary>
public static class WindowCommand
{
    /// <summary>
    /// Executes the window command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0)
            return JsonOutput.Error("Usage: ai-hands window <action> [options]. Actions: find, focus, move, resize, minimize, maximize, restore, wait, list", 2);

        var action = args[0].ToLowerInvariant();
        var cli = new CliParser(args[1..]);
        var useRegex = cli.Has("regex");

        try
        {
            return action switch
            {
                "list" => RunList(),
                "find" => RunFind(cli, useRegex),
                "focus" => RunFocus(cli, useRegex),
                "move" => RunMove(cli, useRegex),
                "resize" => RunResize(cli, useRegex),
                "minimize" => RunMinimize(cli, useRegex),
                "maximize" => RunMaximize(cli, useRegex),
                "restore" => RunRestore(cli, useRegex),
                "wait" => RunWait(cli, useRegex),
                _ => JsonOutput.Error($"Unknown window action: {action}", 2)
            };
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }

    private static int RunList()
    {
        var windows = WindowManager.FindWindows();
        return JsonOutput.Success(new { Windows = windows });
    }

    private static int RunFind(CliParser cli, bool useRegex)
    {
        var title = cli.Positional.FirstOrDefault();
        if (title is null)
            return JsonOutput.Error("Usage: ai-hands window find <title> [--regex]", 2);

        var windows = WindowManager.FindWindows(title, useRegex);
        return JsonOutput.Success(new { Windows = windows });
    }

    private static IntPtr ResolveHandle(CliParser cli, bool useRegex)
    {
        var handle = cli.Get("handle");
        if (handle is not null)
            return new IntPtr(Convert.ToInt64(handle, 16));

        var title = cli.Positional.FirstOrDefault()
            ?? throw new InvalidOperationException("Provide a window title or --handle");
        return WindowManager.FindWindowHandle(title, useRegex);
    }

    private static int RunFocus(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        WindowManager.FocusWindow(hwnd);
        var info = WindowManager.GetWindowInfo(hwnd);
        return JsonOutput.Success(new { Window = info });
    }

    private static int RunMove(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        if (cli.Positional.Count < 3)
            return JsonOutput.Error("Usage: ai-hands window move <title> <x> <y>", 2);
        int x = int.Parse(cli.Positional[1]);
        int y = int.Parse(cli.Positional[2]);
        WindowManager.MoveWindowTo(hwnd, x, y);
        var info = WindowManager.GetWindowInfo(hwnd);
        return JsonOutput.Success(new { Window = info });
    }

    private static int RunResize(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        if (cli.Positional.Count < 3)
            return JsonOutput.Error("Usage: ai-hands window resize <title> <w> <h>", 2);
        int w = int.Parse(cli.Positional[1]);
        int h = int.Parse(cli.Positional[2]);
        WindowManager.ResizeWindowTo(hwnd, w, h);
        var info = WindowManager.GetWindowInfo(hwnd);
        return JsonOutput.Success(new { Window = info });
    }

    private static int RunMinimize(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        WindowManager.MinimizeWindow(hwnd);
        return JsonOutput.Success(new { Action = "minimized" });
    }

    private static int RunMaximize(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        WindowManager.MaximizeWindow(hwnd);
        return JsonOutput.Success(new { Action = "maximized" });
    }

    private static int RunRestore(CliParser cli, bool useRegex)
    {
        var hwnd = ResolveHandle(cli, useRegex);
        WindowManager.RestoreWindow(hwnd);
        return JsonOutput.Success(new { Action = "restored" });
    }

    private static int RunWait(CliParser cli, bool useRegex)
    {
        var title = cli.Positional.FirstOrDefault();
        if (title is null)
            return JsonOutput.Error("Usage: ai-hands window wait <title> [--timeout ms]", 2);

        int timeout = cli.GetInt("timeout", 10000);
        var window = WindowManager.WaitForWindow(title, timeout, useRegex);

        if (window is null)
            return JsonOutput.Error($"Timeout waiting for window '{title}'", 3);

        return JsonOutput.Success(new { Window = window });
    }
}
