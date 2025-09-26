using Device.IService;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.ViewModels
{
    public class HistoricalDataViewModel: BindableBase
    {
        private readonly IMySQLDataService _mySQLDataService;
        private readonly ILogger<HistoricalDataViewModel> _logger;
        private ObservableCollection<DeviceDataInfos> _deviceDataHistory;

        public ObservableCollection<DeviceDataInfos> DeviceDataHistory
        {
            get => _deviceDataHistory;
            set => SetProperty(ref _deviceDataHistory, value);
        }

        public DelegateCommand QueryCommand { get; }

        public HistoricalDataViewModel(ILogger<HistoricalDataViewModel> logger, IMySQLDataService mySQLDataService)
        {
            _logger = logger;
            _mySQLDataService = mySQLDataService;
            QueryCommand = new DelegateCommand(LoadDeviceDataHistory);
            LoadDeviceDataHistory();
        }

        private async void LoadDeviceDataHistory()
        {
            try
            {
                var data = await _mySQLDataService.GetDeviceDataHistory();
                DeviceDataHistory = new ObservableCollection<DeviceDataInfos>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Sourse of MySQLDataService.GetDeviceDataHistory()");
            }
        
        }
    }
}
