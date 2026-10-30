namespace Wada.SOReplaceLabel.Application.LabelPrinter;

public interface IPrintLabelUseCase
{
    Task ExecuteAsync(string content);
}
