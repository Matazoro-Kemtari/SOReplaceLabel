using System.Windows;
using Livet.Messaging;
using Livet.Messaging.IO;

namespace Wada.SOReplaceLabel.Wpf.ViewModels;

public class MessageNotificationViaLivet
{
    private const string DefaultTitle = "SO情報自動印刷";

    public static InformationMessage MakeErrorMessage(string message, string title = DefaultTitle) => new(
        message, title, MessageBoxImage.Error, "Info");

    public static InformationMessage MakeExclamationMessage(string message, string title = DefaultTitle) => new(
        message, title, MessageBoxImage.Exclamation, "Info");

    public static InformationMessage MakeInformationMessage(string message, string title = DefaultTitle) => new(
        message, title, MessageBoxImage.Information, "Info");

    public static ConfirmationMessage MakeQuestionMessage(string message, string title = DefaultTitle) => new(
        message, title, MessageBoxImage.Question, "Confirm")
    { Button = MessageBoxButton.YesNo };

    public static SavingFileSelectionMessage MakeSaveFileDialog() => new("SaveFiling");
}
