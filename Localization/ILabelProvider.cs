using Localization.Attributes;
using Localization.Sections;

namespace Localization;

public interface ILabelProvider
{
    [ResourceId("j")] JobsSection Jobs { get; }
    [ResourceId("t")] TechnologiesSection Technologies { get; }
}