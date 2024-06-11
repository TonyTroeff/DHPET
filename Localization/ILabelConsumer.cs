namespace Localization;

public interface ILabelConsumer
{
    string GetLabel(ILabelProvider labelProvider);
}