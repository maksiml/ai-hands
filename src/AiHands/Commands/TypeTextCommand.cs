using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "type" CLI command for simulating keyboard text input.
/// </summary>
public static class TypeTextCommand
{
    /// <summary>
    /// Executes the type command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);

        if (cli.Positional.Count == 0)
            return JsonOutput.Error("Usage: ai-hands type <text> [--delay ms]", 2);

        var text = cli.Positional[0];
        var delay = cli.GetInt("delay", 10);

        try
        {
            InputSimulator.TypeText(text, delay);
            return JsonOutput.Success(new { Length = text.Length, Text = text });
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
