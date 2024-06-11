using Localization.Attributes;

namespace Localization.Sections;

public class JobsSection
{
    [ResourceId("dev")] public string Dev([ResourceParameter("xp")] string experience) => $"{experience} Software Developer";
    [ResourceId("qa")] public string QA => "Quality Assurance Engineer";
    [ResourceId("pm")] public string PM => "Project Manager";
    [ResourceId("test")] public string Test => "Test";
}