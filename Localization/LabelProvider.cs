using Localization.Sections;

namespace Localization;

public class LabelProvider : ILabelProvider
{
    public JobsSection Jobs { get; } = new();
    public TechnologiesSection Technologies { get; } = new();
}