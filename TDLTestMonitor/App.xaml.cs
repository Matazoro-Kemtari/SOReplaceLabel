using System.Windows;
using Wada.SOReplaceLabel.Application.LabelPrinter;
using Wada.SOReplaceLabel.Domain;
using Wada.SOReplaceLabel.Infrastructure.StarIO;
using Wada.SOReplaceLabel.Wpf;

namespace TDLTestMonitor;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.Register<IPrintLabelUseCase, PrintLabelUseCase>();
        containerRegistry.Register<ILabelPrinter, LabelPrinter>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);

        // Moduleを読み込む
        moduleCatalog.AddModule<SOReplaceLabelModule>(InitializationMode.WhenAvailable);
    }
}

