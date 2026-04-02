using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "key" CLI command for simulating key combinations.
/// </summary>
public static class KeyPressCommand
{
    /// <summary>
    /// Executes the key press command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);

        if (cli.Positional.Count == 0)
            return JsonOutput.Error("Usage: ai-hands key <keys> (e.g. ctrl+s, alt+f4, enter)", 2);

        var keys = cli.Positional[0];

        try
        {
            InputSimulator.KeyPress(keys);
            return JsonOutput.Success(new { Keys = keys });
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
