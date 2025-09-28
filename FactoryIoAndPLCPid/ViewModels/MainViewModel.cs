using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FactoryIoAndPLCPid.ViewModels
{
    public class MainViewModel:BindableBase
    {

        private readonly ILogger<MainViewModel> _logger;
        public System.Timers.Timer timer=new System.Timers.Timer(1000);

        private string _time=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string Time
        {
            get { return _time; }
            set { SetProperty(ref _time, value); }
        }

        public DelegateCommand<Window> MaximizeORNormalCommand { get; set; }

        public DelegateCommand<Window> MinimizeCommand { get; set; }

        public DelegateCommand<Window> CloseCommand { get; set; }

        public DelegateCommand<string> NavigateCommand { get; private set; } 

        private readonly IRegionManager _regionManager;
        public MainViewModel(ILogger<MainViewModel> logger, IRegionManager regionManager)
        {
            _logger= logger;
            _logger.LogInformation("APP is starting");
            try
            {
                timer.Start();
                timer.Elapsed += (sender, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    });

                };
                MaximizeORNormalCommand = new DelegateCommand<Window>(MaximizeORNormal);
                MinimizeCommand = new DelegateCommand<Window>(window => window.WindowState = WindowState.Minimized);
                CloseCommand = new DelegateCommand<Window>(window =>
                {
                    window.Close();
                    timer.Stop();
                    Application.Current.Shutdown();
                }
                );
                _regionManager = regionManager;
                NavigateCommand = new DelegateCommand<string>(
                    viewName => _regionManager.RequestNavigate("ContentRegion", viewName),
                    viewName => !string.IsNullOrEmpty(viewName)
                    );
            }
            catch (Exception ex)
            {
             _logger.LogError(ex.Message,"Sourse of the MainViewModel");
            }
          
        }

       

        private void MaximizeORNormal(Window window)
        {
            if (window != null)
            {
                var workArea = SystemParameters.WorkArea;
                window.MaxWidth = workArea.Width+10;
                window.MaxHeight = workArea.Height+10;

                if (window.WindowState==WindowState.Normal)
                {
                    window.WindowState = WindowState.Maximized;
                }
                else
                {
                    window.WindowState = WindowState.Normal;
                }
            }
        }
    }
}
