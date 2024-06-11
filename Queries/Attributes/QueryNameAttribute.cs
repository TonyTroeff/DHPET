namespace Queries.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class QueryNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}