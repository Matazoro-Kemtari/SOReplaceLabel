using Wada.SOReplaceLabel.Domain.ShopOrderAggregation;

namespace Wada.SOReplaceLabel.Domain;

public interface ILabelPrinter
{
    Task PrintAsync(LabelPrintContent content);
}
