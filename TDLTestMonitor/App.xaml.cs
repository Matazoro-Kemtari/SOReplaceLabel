using System.Configuration;
using System.Data;
using System.Windows;
using Prism.DryIoc;
using Wada.SOReplaceLabel.Wpf;
using Wada.SOReplaceLabel.Wpf.ViewModels;

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
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);

        // Moduleを読み込む
        moduleCatalog.AddModule<SOReplaceLabelModule>(InitializationMode.WhenAvailable);
    }
}

