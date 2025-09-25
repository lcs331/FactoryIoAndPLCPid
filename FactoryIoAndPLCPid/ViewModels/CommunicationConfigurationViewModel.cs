using Device.IService;
using Device.Service;
using FactoryIoAndPLCPid.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FactoryIoAndPLCPid.ViewModels
{
    public class CommunicationConfigurationViewModel:BindableBase,IDisposable
    {

        private readonly IGetSystemDataService _systemInfo;
        private readonly CommunicationManager _comm;
        private readonly SynchronizationContext _uiContext;

        public AsyncDelegateCommand ConnectCommand { get; }
        public AsyncDelegateCommand DisconnectCommand { get; }

        public ObservableCollection<string> IpAddresses { get; } = new ObservableCollection<string>();

        private string _ipName = "192.168.1.10";
        public string IpName { get => _ipName; set => SetProperty(ref _ipName, value); }

        private string _portName = "502";
        public string PortName { get => _portName; set => SetProperty(ref _portName, value); }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }

        private string _heartbeatMessage = "无心跳";
        public string HeartbeatMessage { get => _heartbeatMessage; set => SetProperty(ref _heartbeatMessage, value); }

        // stats
        private string _readSpeedRate = "0次/s";
        public string ReadSpeedRate { get => _readSpeedRate; set => SetProperty(ref _readSpeedRate, value); }
        private string _writeSpeedRate = "0次/s";
        public string WriteSpeedRate { get => _writeSpeedRate; set => SetProperty(ref _writeSpeedRate, value); }
        private string _successRate = "0%";
        public string ReadAndWriteSuccessRate { get => _successRate; set => SetProperty(ref _successRate, value); }

        public CommunicationConfigurationViewModel(IGetSystemDataService systemInfoService, IConfigurationService configurationService)
        {
            _systemInfo = systemInfoService;
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _comm = new CommunicationManager(configurationService);

            // subscribe
            _comm.ConnectionChanged += (connected) => _uiContext.Post(_ => IsConnected = connected, null);
            _comm.HeartbeatChanged += (hb) => _uiContext.Post(_ => HeartbeatMessage = hb ? "心跳正常" : "心跳异常", null);
            _comm.StatsUpdated += (r, w, s) => _uiContext.Post(_ =>
            {
                ReadSpeedRate = $"{r}次/s";
                WriteSpeedRate = $"{w}次/s";
                ReadAndWriteSuccessRate = $"{s}%";
            }, null);

            // commands (use Async handlers if Prism supports FromAsyncHandler)
            ConnectCommand =new AsyncDelegateCommand(async () =>
            {
                int port = int.TryParse(PortName, out var p) ? p : 502;
                await _comm.ConnectAsync(IpName, port);
                _comm.Start(IpName, port, autoReconnect: true); // start background loop if not started
            });

            DisconnectCommand = new AsyncDelegateCommand(async () =>
            {
                await _comm.DisconnectAsync();
                await _comm.StopAsync();
            });

            // system info polling - simple timer instead of background Task.Run loops
            var timer = new System.Timers.Timer(1000) { AutoReset = true };
            timer.Elapsed += (_, __) =>
            {
                try
                {
                    var cpu = _systemInfo.GetCpu();
                    var mem = _systemInfo.GetMemory();
                    _uiContext.Post(_ =>
                    {
                        // your CpuUsage / MemoryUsage properties (not shown) set here
                    }, null);
                }
                catch { /* log */ }
            };
            timer.Start();
        }

        public void Dispose()
        {
            // stop comm and dispose resources
            _comm?.StopAsync().Wait(1000);
            _comm?.Dispose();
        }



    }


}

