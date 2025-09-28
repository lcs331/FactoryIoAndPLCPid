using Device.IService;
using Device.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Models;
using Prism.Ioc;
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
    public class HistoricalDataViewModel : BindableBase
    {
        private readonly IMySQLDataService _mySQLDataService;
        private readonly ILogger<HistoricalDataViewModel> _logger;
        private readonly IExportExcel _exportExcel;
        private ObservableCollection<DeviceDataInfos> _deviceDataHistory;

        public ObservableCollection<DeviceDataInfos> DeviceDataHistory
        {
            get => _deviceDataHistory;
            set => SetProperty(ref _deviceDataHistory, value);
        }
        private int pageNumber = 1;
        private int pageSize = 10;
        int totalpages = 0;

        private int _currentPage = 1;
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

        private string _excelResultMessage;
        public string ExcelResultMessage
        {
            get => _excelResultMessage;
            set => SetProperty(ref _excelResultMessage, value);
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

        public DelegateCommand ExportCommand { get; }

        public HistoricalDataViewModel(IExportExcel exportExcel, ILogger<HistoricalDataViewModel> logger, IMySQLDataService mySQLDataService)
        {
            _exportExcel=exportExcel;
            _logger = logger;
            _mySQLDataService = mySQLDataService;
            DeviceDataHistory = new ObservableCollection<DeviceDataInfos>();
            //init data
            Task.Run(async () => await LoadPage(pageNumber, pageSize));
            //commands
            QueryCommand = new DelegateCommand(async () => await LoadPage(pageNumber, pageSize)
               , () => _canQuery
            );
            FirstPageCommand = new DelegateCommand(async () => await LoadPage(1, pageSize)
              , () => _canFirstPage);
            NextPageCommand = new DelegateCommand(async () => await LoadPage(CurrentPage + 1, pageSize)
              , () => _canNextPage);
            PrevPageCommand = new DelegateCommand(async () => await LoadPage(CurrentPage - 1, pageSize)
                   , () => _canPrevPage);
                LastPageCommand = new DelegateCommand(async () => await LoadPage(totalpages, pageSize)
                , () => _canLastPage
                );
            ToPageCommand = new DelegateCommand(async () => await LoadPage(CurrentPage, pageSize)
            , () => _canToPage);
            ExportCommand=new DelegateCommand(async ()=>await ExportToExcel());

        }

        private async Task ExportToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel文件|*.xlsx";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "设备历史数据";
            var newresult = await _mySQLDataService.GetDeviceDataHistory();


            ExcelResultMessage="正在导出数据，请稍后...";
            // 表头
            List<string> headers = new List<string>
            {
                "ID",
                "设备状态",
                "设备水位",
                "出水速率",
                "进水速率",
                "停止指令",
                "进水阀门",
                "出水阀门",
                "子站在线",
                "设备时间"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                bool result = await _exportExcel.ExportToExcel<DeviceDataInfos>(saveFileDialog.FileName
                    , headers, newresult.ToList()
                    );
                if (result)
                {
                    ExcelResultMessage = "导出成功！";
                }
                else
                {
                    ExcelResultMessage = "导出失败！";
                }

            }
        }

        private async Task LoadPage(int pageNumber, int PageSize)
        {
            try
            {
                DeviceDataHistory.Clear();
                var totalCount = await _mySQLDataService.GetDeviceDataCountAsync();
                //注释：
                totalpages = (totalCount + pageSize - 1) / pageSize;

                if (pageNumber < 1) pageNumber = 1;
                if (pageNumber > totalpages) pageNumber = totalpages;
                if (totalCount > 0)
                    _canQuery = true;
                else
                    _canQuery = false;
                CurrentPage = pageNumber;
                if (CurrentPage == 1)
                    _canFirstPage = false;
                else
                    _canFirstPage = true;
                if (CurrentPage == totalpages)
                    _canLastPage = false;
                else
                    _canLastPage = true;
                if (CurrentPage == 1)
                    _canPrevPage = false;
                else
                    _canPrevPage = true;
                if (CurrentPage == totalpages)
                    _canNextPage = false;
                else
                    _canNextPage = true;
                _canToPage = true;

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
