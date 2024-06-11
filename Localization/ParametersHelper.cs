namespace Localization;

internal static class ParametersHelper
{
    internal static T? GetValueOrDefault<T>(IDictionary<string, object?> parameters, string key)
    {
        if (parameters.TryGetValue(key, out var value) && value is T typedValue) return typedValue;
        return default;
    }
}