using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "screenshot" CLI command for capturing screen, window, or region images.
/// </summary>
public static class ScreenshotCommand
{
    /// <summary>
    /// Executes the screenshot command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);
        var output = cli.Get("output", "o");
        if (output is null)
            return JsonOutput.Error("--output (-o) is required", 2);

        var delay = cli.GetInt("delay");
        if (delay > 0)
            Thread.Sleep(delay);

        var window = cli.Get("window");
        var handle = cli.Get("handle");
        var region = cli.Get("region");

        try
        {
            if (region is not null)
            {
                var parts = region.Split(',');
                if (parts.Length != 4)
                    return JsonOutput.Error("--region must be x,y,w,h", 2);

                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int w = int.Parse(parts[2]);
                int h = int.Parse(parts[3]);

                var (file, width, height) = ScreenCapture.CaptureRegion(x, y, w, h, output);
                return JsonOutput.Success(new { File = file, Width = width, Height = height });
            }

            if (window is not null)
            {
                var hwnd = WindowManager.FindWindowHandle(window);
                var (file, width, height) = ScreenCapture.CaptureWindow(hwnd, output);
                return JsonOutput.Success(new { File = file, Width = width, Height = height });
            }

            if (handle is not null)
            {
                var hwnd = new IntPtr(Convert.ToInt64(handle, 16));
                var (file, width, height) = ScreenCapture.CaptureWindow(hwnd, output);
                return JsonOutput.Success(new { File = file, Width = width, Height = height });
            }

            {
                var (file, width, height) = ScreenCapture.CaptureFullScreen(output);
                return JsonOutput.Success(new { File = file, Width = width, Height = height });
            }
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
