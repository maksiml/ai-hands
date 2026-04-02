using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "click" CLI command for simulating mouse clicks at screen coordinates.
/// </summary>
public static class ClickCommand
{
    /// <summary>
    /// Executes the click command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);

        if (cli.Positional.Count < 2)
            return JsonOutput.Error("Usage: ai-hands click <x> <y> [--button left|right|middle] [--double] [--window <title>]", 2);

        if (!int.TryParse(cli.Positional[0], out int x) || !int.TryParse(cli.Positional[1], out int y))
            return JsonOutput.Error("x and y must be integers", 2);

        var buttonStr = cli.Get("button") ?? "left";
        var button = buttonStr.ToLowerInvariant() switch
        {
            "left" => MouseButton.Left,
            "right" => MouseButton.Right,
            "middle" => MouseButton.Middle,
            _ => MouseButton.Left
        };
        var doubleClick = cli.Has("double");
        var windowTitle = cli.Get("window");

        try
        {
            if (windowTitle is not null)
            {
                var hwnd = WindowManager.FindWindowHandle(windowTitle);
                WindowManager.FocusWindow(hwnd);
                var info = WindowManager.GetWindowInfo(hwnd);
                // Convert window-relative coords to screen coords
                x += info.X;
                y += info.Y;
            }

            InputSimulator.Click(x, y, button, doubleClick);
            return JsonOutput.Success(new { X = x, Y = y, Button = buttonStr, Double = doubleClick });
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
