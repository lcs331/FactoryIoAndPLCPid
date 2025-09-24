using System.Configuration;
using System.Data;
using System.Windows;
using Device.IService;
using Device.Service;
using FactoryIoAndPLCPid.ViewModels;
using FactoryIoAndPLCPid.views;
using Prism.Ioc;
namespace FactoryIoAndPLCPid
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
           return Container.Resolve<MainView>();
        }
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            // 手动指定 View 与 ViewModel 的绑定规则
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
            ViewModelLocationProvider.Register<CommunicationConfigurationView, CommunicationConfigurationViewModel>();
            ViewModelLocationProvider.Register<ErrorAndLogView, ErrorAndLogViewModel>();
            ViewModelLocationProvider.Register<HistoricalDataView, HistoricalDataViewModel>();
            ViewModelLocationProvider.Register<RealTimeMonitoringView, RealTimeMonitoringViewModel>();
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册
            containerRegistry.Register<IGetSystemDataService, GetSystemDataService>();
            containerRegistry.Register<IConfigurationService, ConfigurationService>();
            containerRegistry.Register<IRealTimeMonitoringService, RealTimeMonitoringService>();
            containerRegistry.RegisterForNavigation<CommunicationConfigurationView>();
            containerRegistry.RegisterForNavigation<ErrorAndLogView>();
            containerRegistry.RegisterForNavigation<HistoricalDataView>();
            containerRegistry.RegisterForNavigation<RealTimeMonitoringView>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var resourceManager= Container.Resolve<IRegionManager>();
            //resourceManager.RequestNavigate("ContentRegion", "RealTimeMonitoringView");
            //resourceManager.RequestNavigate("ContentRegion", "HistoricalDataView");
            //resourceManager.RequestNavigate("ContentRegion", "ErrorAndLogView");
            resourceManager.RequestNavigate("ContentRegion", "CommunicationConfigurationView");

        }


    }

}
