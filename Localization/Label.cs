namespace Localization;

public class Label(string identifier)
{
    public string Identifier { get; } = string.IsNullOrWhiteSpace(identifier) ? throw new ArgumentNullException(nameof(identifier)) : identifier;
    public IDictionary<string, object?> Parameters { get; } = new Dictionary<string, object?>();

    public override string ToString()
        => $"Identifier: {this.Identifier}{Environment.NewLine}Parameters: {{ {string.Join(", ", this.Parameters.Select(x => $"{x.Key}: {x.Value}"))} }}";
}