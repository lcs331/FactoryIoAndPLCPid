using Device.IService;
using Device.Service;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        private int pageNumber  = 1;     
        private int pageSize = 10;
        int totalpages = 0;

        private int _currentPage=1;
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        //命令的可执行状态
        private bool _canQuery = true;
        private bool _canNextPage = true;
        private bool _canPrevPage = true;
        private bool _canFirstPage = true;
        private bool _canLastPage = true;
        private bool _canToPage = true;


        //commands
        public DelegateCommand QueryCommand { get; }
        public DelegateCommand NextPageCommand { get; }
        public DelegateCommand PrevPageCommand { get; }
        public DelegateCommand FirstPageCommand { get; }
        public DelegateCommand LastPageCommand { get; }

        public DelegateCommand ToPageCommand { get; }

        public HistoricalDataViewModel(ILogger<HistoricalDataViewModel> logger, IMySQLDataService mySQLDataService)
        {
           
            _logger = logger;
            _mySQLDataService = mySQLDataService;
            DeviceDataHistory = new ObservableCollection<DeviceDataInfos>();
            //init data
            Task.Run(async () => await LoadPage(pageNumber, pageSize));
            //commands
            QueryCommand = new DelegateCommand(async () => await LoadPage(pageNumber, pageSize));
            FirstPageCommand=new DelegateCommand(async () => await LoadPage(1, pageSize));
            NextPageCommand = new DelegateCommand(async () => await LoadPage(CurrentPage + 1, pageSize));
            PrevPageCommand = new DelegateCommand(async () => await LoadPage(CurrentPage - 1, pageSize));
            if (totalpages > 0)
            LastPageCommand = new DelegateCommand(async () => await LoadPage(totalpages, pageSize));
            
            ToPageCommand=new DelegateCommand(async () => await LoadPage(CurrentPage, pageSize));

         
        }
        private void CanExecuteChanged()
        {

        }



        private async Task LoadPage(int pageNumber,int PageSize)
        {
            try
            {
                DeviceDataHistory.Clear();
                var totalCount = await _mySQLDataService.GetDeviceDataCountAsync();
                //注释：
                totalpages = (totalCount + pageSize - 1) / pageSize;

                if (pageNumber < 1) pageNumber = 1;
                if (pageNumber > totalpages) pageNumber = totalpages;

                CurrentPage = pageNumber;

                var data = await _mySQLDataService.GetDeviceDataPageAsync(pageNumber, pageSize);

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {

                    foreach (var item in data)
                        DeviceDataHistory.Add(item);
                });

                SearchText = $"{CurrentPage}/{totalpages}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadPage");
            }
          

        }
 


    }
}
