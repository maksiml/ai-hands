namespace AiHands.Infrastructure;

/// <summary>
/// Parses command-line arguments into positional and named parameters.
/// </summary>
public class CliParser
{
    /// <summary>
    /// Positional arguments (those not prefixed with -- or -).
    /// </summary>
    public List<string> Positional { get; } = new();

    /// <summary>
    /// Named arguments keyed by flag name (case-insensitive).
    /// </summary>
    public Dictionary<string, string> Named { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new parser from the given argument array.
    /// </summary>
    public CliParser(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var key = args[i][2..];
                if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    Named[key] = args[++i];
                else
                    Named[key] = "true";
            }
            else if (args[i].StartsWith("-") && args[i].Length == 2)
            {
                var key = args[i][1..];
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    Named[key] = args[++i];
                else
                    Named[key] = "true";
            }
            else
            {
                Positional.Add(args[i]);
            }
        }
    }

    /// <summary>
    /// Gets a named argument value by its primary name or optional alias.
    /// </summary>
    public string? Get(string name, string? alias = null)
    {
        if (Named.TryGetValue(name, out var value)) return value;
        if (alias is not null && Named.TryGetValue(alias, out value)) return value;
        return null;
    }

    /// <summary>
    /// Returns true if the named flag is present.
    /// </summary>
    public bool Has(string name) => Named.ContainsKey(name);

    /// <summary>
    /// Gets a named argument parsed as an integer, or returns the default value.
    /// </summary>
    public int GetInt(string name, int defaultValue = 0)
    {
        var val = Get(name);
        return val is not null && int.TryParse(val, out var result) ? result : defaultValue;
    }
}
