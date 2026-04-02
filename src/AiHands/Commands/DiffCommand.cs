using AiHands.Automation;
using AiHands.Infrastructure;

namespace AiHands.Commands;

/// <summary>
/// Handles the "diff" CLI command for comparing two images pixel-by-pixel.
/// </summary>
public static class DiffCommand
{
    /// <summary>
    /// Executes the diff command with the given arguments.
    /// </summary>
    public static int Run(string[] args)
    {
        var cli = new CliParser(args);

        if (cli.Positional.Count < 2)
            return JsonOutput.Error("Usage: ai-hands diff <image1> <image2> [--output path] [--threshold 0-255]", 2);

        var path1 = cli.Positional[0];
        var path2 = cli.Positional[1];
        var output = cli.Get("output", "o");
        int threshold = cli.GetInt("threshold", 10);

        if (!File.Exists(path1))
            return JsonOutput.Error($"File not found: {path1}", 1);
        if (!File.Exists(path2))
            return JsonOutput.Error($"File not found: {path2}", 1);

        try
        {
            var result = ImageDiff.Compare(path1, path2, threshold, output);
            return JsonOutput.Success(result);
        }
        catch (Exception ex)
        {
            return JsonOutput.Error(ex.Message);
        }
    }
}
