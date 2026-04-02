using System.Runtime.InteropServices;
using AiHands.Commands;
using AiHands.Infrastructure;

[DllImport("user32.dll")]
static extern bool SetProcessDPIAware();
SetProcessDPIAware();

if (args.Length == 0)
{
    ShowHelp();
    return 0;
}

try
{
    return args[0].ToLowerInvariant() switch
    {
        "screenshot" => ScreenshotCommand.Run(args[1..]),
        "click" => ClickCommand.Run(args[1..]),
        "type" => TypeTextCommand.Run(args[1..]),
        "key" => KeyPressCommand.Run(args[1..]),
        "scroll" => ScrollCommand.Run(args[1..]),
        "window" => WindowCommand.Run(args[1..]),
        "element" => ElementCommand.Run(args[1..]),
        "diff" => DiffCommand.Run(args[1..]),
        "help" or "--help" or "-h" => ShowHelp(),
        _ => JsonOutput.Error($"Unknown command: {args[0]}", 2)
    };
}
catch (Exception ex)
{
    return JsonOutput.Error(ex.Message);
}

static int ShowHelp()
{
    Console.WriteLine("""
    ai-hands — Windows Desktop Automation CLI

    Usage: ai-hands <command> [options]

    Commands:
      screenshot   Capture screen, window, or region to PNG
      click        Mouse click at coordinates
      type         Type text string
      key          Press key combination
      scroll       Mouse wheel scroll
      window       Window management (find, focus, move, resize, etc.)
      element      UI Automation element discovery and interaction
      diff         Compare two images

    Use 'ai-hands <command> --help' for command-specific help.
    """);
    return 0;
}
