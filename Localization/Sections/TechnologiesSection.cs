using Localization.Attributes;

namespace Localization.Sections;

public class TechnologiesSection
{
    [ResourceId("c#")]
    public string CSharp([ResourceParameter("asp")] bool withAsp, [ResourceParameter("ef")] bool withEf, [ResourceParameter("blazor")] bool withBlazor)
    {
        var result = new List<string> { "C#" };
        if (withAsp) result.Add("ASP.NET Core");
        if (withEf) result.Add("EF Core");
        if (withBlazor) result.Add("Blazor");

        return string.Join("; ", result);
    }
}