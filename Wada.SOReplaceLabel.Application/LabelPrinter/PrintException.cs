namespace Wada.SOReplaceLabel.Application.LabelPrinter;

public class PrintException : ApplicationException
{
    public PrintException()
    {
    }

    public PrintException(string message) : base(message)
    {
    }

    public PrintException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
