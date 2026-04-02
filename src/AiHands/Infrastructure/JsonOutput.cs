using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiHands.Infrastructure;

/// <summary>
/// Provides standardized JSON output formatting for CLI responses.
/// </summary>
public static class JsonOutput
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Writes a success JSON response to stdout and returns exit code 0.
    /// </summary>
    public static int Success(object data)
    {
        var wrapper = new Dictionary<string, object> { ["ok"] = true };
        foreach (var prop in data.GetType().GetProperties())
        {
            var name = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
            var value = prop.GetValue(data);
            if (value is not null)
                wrapper[name] = value;
        }
        Console.WriteLine(JsonSerializer.Serialize(wrapper, Options));
        return 0;
    }

    /// <summary>
    /// Writes an error JSON response to stdout and returns the specified exit code.
    /// </summary>
    public static int Error(string message, int exitCode = 1)
    {
        var error = new { ok = false, error = message };
        Console.WriteLine(JsonSerializer.Serialize(error, Options));
        return exitCode;
    }
}
