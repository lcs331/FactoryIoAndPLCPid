using Device.IService;
using Device.Service;
using FactoryIoAndPLCPid.Models;
using LiveCharts;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FactoryIoAndPLCPid.ViewModels
{
    public class RealTimeMonitoringViewModel:BindableBase,IDisposable
    {
        
        private CancellationTokenSource _cts = new();
        private readonly ILogger<RealTimeMonitoringViewModel> _logger;
        private readonly IRealTimeMonitoringService realTimeMonitoringService;
        private readonly IMySQLDataService _mySQLDataService;
        public ObservableCollection<PropertyCard> Cards { get; set; } = new ObservableCollection<PropertyCard>();

        public DeviceDataInfos? CurrentDevice { get; set; }

        private DeviceDataInfos? _lastDeviceData;

        public List<string> XLabels { get; set; } = new List<string>();

        public ChartValues<float> WaterLevelValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> OutflowValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> InletValveValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> OutletValveValues { get; set; } = new ChartValues<float>();

        public ObservableCollection<LineLegendItem> LineLegendItems { get; set; } = new ObservableCollection<LineLegendItem>();

        private string message="等待设备数据...";
        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }


        public RealTimeMonitoringViewModel(IMySQLDataService mySQLDataService, ILogger<RealTimeMonitoringViewModel> logger, IRealTimeMonitoringService realTimeService)
        {
            _logger=logger;
            realTimeMonitoringService = realTimeService;
            _mySQLDataService = mySQLDataService;
            _cts = new CancellationTokenSource();
            // 图例颜色
            LineLegendItems.Add(new LineLegendItem { Name = "水位", Color = Brushes.LightBlue });
            LineLegendItems.Add(new LineLegendItem { Name = "出水速率", Color = Brushes.Orange });
            LineLegendItems.Add(new LineLegendItem { Name = "进水阀门", Color = Brushes.Green });
            LineLegendItems.Add(new LineLegendItem { Name = "出水阀门", Color = Brushes.White });
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                       
                        var de = realTimeMonitoringService.GetDeciveDataInfos();
                        if (de != null)
                        {
                            CurrentDevice = de;
                            await Application.Current.Dispatcher.BeginInvoke( async () =>
                            {
                                LoadDeviceData(de);// 转成卡片集合                            
                                AddPoint(de);// 加入到图表中
                              
                            });

                            _lastDeviceData = de;
                        }
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, "Sourse of RealTimeMonitoringViewModel");

                    }
                }              
            }, _cts.Token);

            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        if (_lastDeviceData != null
                            && _mySQLDataService!= null
                            && _lastDeviceData.StartInstruction == 1
                            && _lastDeviceData.StopInstruction == 0
                            && _lastDeviceData.DeviceStart == 1)
                        {
                            await _mySQLDataService.InsertDeviceData(_lastDeviceData);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Database insert failed");
                    }

                    await Task.Delay(3000); // 每3秒执行一次(测试)
                }
            }, _cts.Token);



        }

        public void AddPoint(DeviceDataInfos inputdata)
        {
            PropertyLiveChats data = new PropertyLiveChats();
            // 赋值
            data.WaterLevel=inputdata.EquipmentWaterLevel;
            data.OutflowRate = inputdata.WaterOutflowRate;
            data.OutletValve = inputdata.WaterOutletValve;
            data.InletValve = inputdata.WaterInletValve;
            if (inputdata.EquipmentWaterLevel > 0)
            {
                Message = "设备正常运行..";
            }
            // 时间戳
            XLabels.Add(data.Timestamp.ToString("HH:mm:ss"));
            // 值
            WaterLevelValues.Add(data.WaterLevel);
            OutflowValues.Add(data.OutflowRate);
            InletValveValues.Add(data.InletValve);
            OutletValveValues.Add(data.OutletValve);
            
            // 控制最多显示100个点
            if (XLabels.Count > 100)
            {
                XLabels.RemoveAt(0);
                WaterLevelValues.RemoveAt(0);
                OutflowValues.RemoveAt(0);
                InletValveValues.RemoveAt(0);
                OutletValveValues.RemoveAt(0);
            }
        }


        public  void LoadDeviceData(DeviceDataInfos data)
        {
                  
            Cards.Clear();

            Cards.Add(new PropertyCard
            {
                Title = "设备启动",
                Description = "设备运行状态",
                Value = data.DeviceStart
            });

          
            Cards.Add(new PropertyCard
            {
                Title = "水位",
                Description = "设备水位 m",
                Value = data.EquipmentWaterLevel
            });
            Cards.Add(new PropertyCard
            {
                Title = "出水速率",
                Description = "L/s",
                Value = data.WaterOutflowRate
            });
            Cards.Add(new PropertyCard
            {
                Title = "设备报警",
                Description = "故障代码",
                Value = data.ErrorState
            });

            Cards.Add(new PropertyCard
            {
                Title = "停止指示",
                Description = "1 表示停止",
                Value = data.StopInstruction
            });
            Cards.Add(new PropertyCard
            {
                Title = "启动指示",
                Description = "1 表示启动",
                Value = data.StartInstruction
            });

            Cards.Add(new PropertyCard
            {
                Title = "出水速率",
                Description = "L/s",
                Value = data.WaterOutflowRate
            });

            Cards.Add(new PropertyCard
            {
                Title = "进水阀门",
                Description = "阀门开度 %",
                Value = data.WaterInletValve
            });
         
        }


        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
