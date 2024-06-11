namespace Localization;

public class DynamicLabelConsumer(Func<ILabelProvider, string> labelSelector) : ILabelConsumer
{
    private readonly Func<ILabelProvider, string> _labelSelector = labelSelector ?? throw new ArgumentNullException(nameof(labelSelector));

    public string GetLabel(ILabelProvider labelProvider) => this._labelSelector(labelProvider);
}