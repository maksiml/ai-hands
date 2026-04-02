using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "scroll" CLI command for simulating mouse scroll events.
/// </summary>
public static class ScrollCommand
{
    /// <summary>
    /// Executes the scroll command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);

        if (cli.Positional.Count == 0)
            return JsonOutput.Error("Usage: ai-hands scroll <amount> [--x x] [--y y] [--horizontal]", 2);

        if (!int.TryParse(cli.Positional[0], out int amount))
            return JsonOutput.Error("amount must be an integer (positive=up, negative=down)", 2);

        int x = cli.GetInt("x", -1);
        int y = cli.GetInt("y", -1);
        bool horizontal = cli.Has("horizontal");

        try
        {
            InputSimulator.Scroll(amount, x, y, horizontal);
            return JsonOutput.Success(new
            {
                Amount = amount,
                Direction = horizontal ? "horizontal" : "vertical"
            });
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
