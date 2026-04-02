using System.Windows.Automation;

namespace AiHands.Automation;

/// <summary>
/// Represents a UI Automation element with its properties and bounding rectangle.
/// </summary>
public record ElementInfo(
    string Name,
    string Type,
    string AutomationId,
    int X, int Y, int Width, int Height,
    string? Value = null);

/// <summary>
/// Provides UI Automation helpers for discovering and interacting with UI elements.
/// </summary>
public static class UiaHelper
{
    /// <summary>
    /// Lists UI elements in a window's automation tree, optionally filtered by name, type, or automation ID.
    /// </summary>
    public static List<ElementInfo> ListElements(IntPtr hwnd, int maxDepth = 3,
        string? nameFilter = null, string? typeFilter = null, string? idFilter = null)
    {
        var root = AutomationElement.FromHandle(hwnd);
        var results = new List<ElementInfo>();
        WalkTree(root, 0, maxDepth, nameFilter, typeFilter, idFilter, results);
        return results;
    }

    /// <summary>
    /// Finds the first UI element matching the specified criteria in the window's automation tree.
    /// </summary>
    public static ElementInfo? FindElement(IntPtr hwnd,
        string? name = null, string? type = null, string? automationId = null, int maxDepth = 10)
    {
        var root = AutomationElement.FromHandle(hwnd);
        return FindInTree(root, 0, maxDepth, name, type, automationId);
    }

    /// <summary>
    /// Clicks the center of the specified UI element.
    /// </summary>
    public static void ClickElement(ElementInfo element)
    {
        int cx = element.X + element.Width / 2;
        int cy = element.Y + element.Height / 2;
        InputSimulator.Click(cx, cy);
    }

    /// <summary>
    /// Sets the value of a UI element that supports the ValuePattern.
    /// </summary>
    public static bool SetElementValue(IntPtr hwnd, string? name, string? type, string? automationId, string value)
    {
        var root = AutomationElement.FromHandle(hwnd);
        var element = FindAutomationElement(root, 0, 10, name, type, automationId);
        if (element is null) return false;

        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
        {
            ((ValuePattern)pattern).SetValue(value);
            return true;
        }

        return false;
    }

    private static void WalkTree(AutomationElement element, int depth, int maxDepth,
        string? nameFilter, string? typeFilter, string? idFilter, List<ElementInfo> results)
    {
        if (depth > maxDepth) return;

        var info = ToElementInfo(element);
        if (info is not null && MatchesFilters(info, nameFilter, typeFilter, idFilter))
            results.Add(info);

        try
        {
            var child = TreeWalker.ControlViewWalker.GetFirstChild(element);
            while (child is not null)
            {
                WalkTree(child, depth + 1, maxDepth, nameFilter, typeFilter, idFilter, results);
                child = TreeWalker.ControlViewWalker.GetNextSibling(child);
            }
        }
        catch (ElementNotAvailableException) { }
    }

    private static ElementInfo? FindInTree(AutomationElement element, int depth, int maxDepth,
        string? name, string? type, string? automationId)
    {
        if (depth > maxDepth) return null;

        var info = ToElementInfo(element);
        if (info is not null && MatchesExact(info, name, type, automationId))
            return info;

        try
        {
            var child = TreeWalker.ControlViewWalker.GetFirstChild(element);
            while (child is not null)
            {
                var found = FindInTree(child, depth + 1, maxDepth, name, type, automationId);
                if (found is not null) return found;
                child = TreeWalker.ControlViewWalker.GetNextSibling(child);
            }
        }
        catch (ElementNotAvailableException) { }

        return null;
    }

    private static AutomationElement? FindAutomationElement(AutomationElement element, int depth, int maxDepth,
        string? name, string? type, string? automationId)
    {
        if (depth > maxDepth) return null;

        var info = ToElementInfo(element);
        if (info is not null && MatchesExact(info, name, type, automationId))
            return element;

        try
        {
            var child = TreeWalker.ControlViewWalker.GetFirstChild(element);
            while (child is not null)
            {
                var found = FindAutomationElement(child, depth + 1, maxDepth, name, type, automationId);
                if (found is not null) return found;
                child = TreeWalker.ControlViewWalker.GetNextSibling(child);
            }
        }
        catch (ElementNotAvailableException) { }

        return null;
    }

    private static ElementInfo? ToElementInfo(AutomationElement element)
    {
        try
        {
            var current = element.Current;
            var rect = current.BoundingRectangle;
            if (rect.IsEmpty || double.IsInfinity(rect.X)) return null;

            string? value = null;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
                value = ((ValuePattern)pattern).Current.Value;

            return new ElementInfo(
                current.Name ?? "",
                current.ControlType.ProgrammaticName.Replace("ControlType.", ""),
                current.AutomationId ?? "",
                (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height,
                value);
        }
        catch (ElementNotAvailableException)
        {
            return null;
        }
    }

    private static bool MatchesFilters(ElementInfo info, string? name, string? type, string? id)
    {
        if (name is not null && !info.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            return false;
        if (type is not null && !info.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            return false;
        if (id is not null && !info.AutomationId.Contains(id, StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    private static bool MatchesExact(ElementInfo info, string? name, string? type, string? id)
    {
        // At least one filter must be specified
        if (name is null && type is null && id is null) return false;

        if (name is not null && !info.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            return false;
        if (type is not null && !info.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            return false;
        if (id is not null && !info.AutomationId.Equals(id, StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }
}
