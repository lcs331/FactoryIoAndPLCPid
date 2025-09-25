using Device.IService;
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
        private readonly ILogger<RealTimeMonitoringViewModel> _logger;
        private CancellationTokenSource _cts = new();
        private readonly IRealTimeMonitoringService realTimeMonitoringService;
        public ObservableCollection<PropertyCard> Cards { get; set; } = new ObservableCollection<PropertyCard>();

        public DeciveDataInfos? CurrentDevice { get; set; }

        public List<string> XLabels { get; set; } = new List<string>();

        public ChartValues<float> WaterLevelValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> OutflowValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> InletValveValues { get; set; } = new ChartValues<float>();
        public ChartValues<float> OutletValveValues { get; set; } = new ChartValues<float>();

        public ObservableCollection<LineLegendItem> LineLegendItems { get; set; } = new ObservableCollection<LineLegendItem>();


        public RealTimeMonitoringViewModel(ILogger<RealTimeMonitoringViewModel> logger, IRealTimeMonitoringService realTimeService)
        {
            _logger=logger;
            realTimeMonitoringService = realTimeService;
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
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LoadDeviceData(de);// 转成卡片集合
                                AddPoint(de);// 加入到图表中
                            });

                        }
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, "Sourse of RealTimeMonitoringViewModel");

                    }
                }              
            }, _cts.Token);
        }

        public void AddPoint(DeciveDataInfos inputdata)
        {
            PropertyLiveChats data = new PropertyLiveChats();
            // 赋值
            data.WaterLevel=inputdata.EquipmentWaterLevel;
            data.OutflowRate = inputdata.WaterOutflowEate;
            data.OutletValve = inputdata.WaterOutletValve;
            data.InletValve = inputdata.WaterInletValve;
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


        public void LoadDeviceData(DeciveDataInfos data)
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
                Value = data.WaterOutflowEate
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
                Description = "1 表示停止，0 表示运行",
                Value = data.StopInstruction
            });
            Cards.Add(new PropertyCard
            {
                Title = "启动指示",
                Description = "1 表示启动，0 表示停止",
                Value = data.StartInstruction
            });

            Cards.Add(new PropertyCard
            {
                Title = "出水速率",
                Description = "L/s",
                Value = data.WaterOutflowEate
            });

            Cards.Add(new PropertyCard
            {
                Title = "进水阀门",
                Description = "阀门开度 %",
                Value = data.WaterInletValve
            });

            // 还可以继续把 StopInstruction、DeviceOnline 等属性加进来
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
