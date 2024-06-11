namespace Localization.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ResourceIdAttribute : Attribute
{
    public string Name { get; }

    public ResourceIdAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
    }
}