using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "element" CLI command for listing and finding UI Automation elements.
/// </summary>
public static class ElementCommand
{
    /// <summary>
    /// Executes the element command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        if (args.Length == 0)
            return JsonOutput.Error("Usage: ai-hands element <list|find> <window-title> [options]", 2);

        var action = args[0].ToLowerInvariant();
        var cli = new CliParser(args[1..]);

        try
        {
            return action switch
            {
                "list" => RunList(cli),
                "find" => RunFind(cli),
                _ => JsonOutput.Error($"Unknown element action: {action}", 2)
            };
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }

    private static int RunList(CliParser cli)
    {
        var title = cli.Positional.FirstOrDefault();
        if (title is null)
            return JsonOutput.Error("Usage: ai-hands element list <window-title> [--name x] [--type x] [--id x] [--depth n]", 2);

        var hwnd = WindowManager.FindWindowHandle(title);
        int depth = cli.GetInt("depth", 3);
        var name = cli.Get("name");
        var type = cli.Get("type");
        var id = cli.Get("id");

        var elements = UiaHelper.ListElements(hwnd, depth, name, type, id);
        return JsonOutput.Success(new { Count = elements.Count, Elements = elements });
    }

    private static int RunFind(CliParser cli)
    {
        var title = cli.Positional.FirstOrDefault();
        if (title is null)
            return JsonOutput.Error("Usage: ai-hands element find <window-title> [--name x] [--type x] [--id x] [--click] [--value text]", 2);

        var hwnd = WindowManager.FindWindowHandle(title);
        var name = cli.Get("name");
        var type = cli.Get("type");
        var id = cli.Get("id");

        if (name is null && type is null && id is null)
            return JsonOutput.Error("At least one of --name, --type, or --id is required", 2);

        var element = UiaHelper.FindElement(hwnd, name, type, id);
        if (element is null)
            return JsonOutput.Error("Element not found", 4);

        // Optionally click the element
        if (cli.Has("click"))
            UiaHelper.ClickElement(element);

        // Optionally set a value
        var value = cli.Get("value");
        if (value is not null)
            UiaHelper.SetElementValue(hwnd, name, type, id, value);

        return JsonOutput.Success(new { Element = element });
    }
}
