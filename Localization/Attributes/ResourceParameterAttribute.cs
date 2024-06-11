namespace Localization.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class ResourceParameterAttribute : Attribute
{
    public string Name { get; }

    public ResourceParameterAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
    }
}