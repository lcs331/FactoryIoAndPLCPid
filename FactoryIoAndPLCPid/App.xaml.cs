using Device.IService;
using Device.Service;
using DryIoc.ImTools;
using FactoryIoAndPLCPid.ViewModels;
using FactoryIoAndPLCPid.ViewModels.Common;
using FactoryIoAndPLCPid.views;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Prism.Events;
using Prism.Ioc;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FactoryIoAndPLCPid
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private ILogger<App> _logger;
        private IHost _webHost;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _webHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://192.168.1.13:5000");
                    webBuilder.ConfigureServices(service =>
                        {
                            service.AddControllers();
                            service.AddSingleton<IMySQLDataService, MySQLDataService>();

                        }
                        );

                    webBuilder.Configure(app =>
                     {
                         app.UseRouting();
                         app.UseEndpoints(endpoint =>
                         {
                             endpoint.MapControllers();
                         }
                         );
                     });
                }

                ).Build();

            _webHost.StartAsync();

         


        }
        protected override void OnExit(ExitEventArgs e)
        {
            System.Environment.Exit(0);
            base.OnExit(e);
        }
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
            // 1. 从 NLog.config 加载配置
            var config = new XmlLoggingConfiguration("newNLog.config");
            LogManager.Configuration = config;

            // 2. 创建 ServiceCollection
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
           
                loggingBuilder.AddNLog(config);
            });



            // 3. 构建 ServiceProvider
            var serviceProvider = services.BuildServiceProvider();

            // 4. 把 LoggerFactory 和 ILogger<T> 注册进 Prism 容器
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            containerRegistry.RegisterInstance(loggerFactory);
            containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));
            containerRegistry.Register<IMySQLDataService, MySQLDataService>();
            containerRegistry.Register<IGetSystemDataService, GetSystemDataService>();
            containerRegistry.Register<IConfigurationService, ConfigurationService>();
            containerRegistry.Register<IRealTimeMonitoringService, RealTimeMonitoringService>();
            containerRegistry.Register<IExportExcel, ExportExcel>();
            containerRegistry.RegisterForNavigation<CommunicationConfigurationView>();
            containerRegistry.RegisterForNavigation<ErrorAndLogView>();
            containerRegistry.RegisterForNavigation<HistoricalDataView>();
            containerRegistry.RegisterForNavigation<RealTimeMonitoringView>();



        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _logger = Container.Resolve<ILogger<App>>(); // 解析 logger
            RegisterGlobalExceptionHandlers();

            // ✅ 在这里 Prism 已经完成容器构建，IEventAggregator 可用
            var eventAggregator = Container.Resolve<IEventAggregator>();
            var loggerFactory = Container.Resolve<ILoggerFactory>();

            // ✅ 添加自定义 Provider
            loggerFactory.AddProvider(new EventAggregatorLoggerProvider(eventAggregator));


            var resourceManager = Container.Resolve<IRegionManager>();
            resourceManager.RequestNavigate("ContentRegion", "ErrorAndLogView");
            resourceManager.RequestNavigate("ContentRegion", "CommunicationConfigurationView");

        }

        private void RegisterGlobalExceptionHandlers()
        {
            // UI 线程异常
            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                _logger.LogError(e.Exception, "Dispatcher Thread Exception");
                e.Handled = true;
            };

            // 非 UI 线程异常
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    _logger.LogError(ex, "AppDomain Unhandled Exception");
            };

            // Task 异步未观察到异常
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                _logger.LogError(e.Exception, "Unobserved Task Exception");
                e.SetObserved();
            };
        }
    }

}
