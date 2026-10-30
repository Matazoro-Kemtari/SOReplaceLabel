using Wada.SOReplaceLabel.Wpf.Views;

namespace Wada.SOReplaceLabel.Wpf;

public class SOReplaceLabelModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        var regionManager = containerProvider.Resolve<IRegionManager>();
        regionManager?.RegisterViewWithRegion("ContentRegion", typeof(WebViewPage));
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    { }
}
