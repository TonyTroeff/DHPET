using Queries.Attributes;

namespace Demo.Models;

public class Person(string name, int age, bool isEmployed)
{
    [QueryName("n")] public string Name { get; } = name;
    [QueryName("a")] public int Age { get; } = age;
    [QueryName("e")] public bool IsEmployed { get; } = isEmployed;
}