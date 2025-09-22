using Device.IService;
using Device.Service;
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
    public class CommunicationConfigurationViewModel:BindableBase
    {
        private readonly IGetSystemDataService _systemInfoService;
        private readonly IConfigurationService _configurationService;
        public ObservableCollection<string> IpAddresses { get; set; }

        public DelegateCommand ConnectCommand { get; set; }

        public DelegateCommand DisconnectCommand { get; set; }

        private double cpuUsage=20;
        public double CpuUsage
        {
            get { return cpuUsage; }
            set { SetProperty(ref cpuUsage, value); }
        }
        private double memoryUsage=50;
        public double MemoryUsage
        {
            get { return memoryUsage; }
            set { SetProperty(ref memoryUsage, value); }
        }

        private string ipName="192.168.1.10";
        public string IpName
        {
            get { return ipName; }
            set { SetProperty(ref ipName, value); }
        }
        private bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }
        private bool heartbeat = false;
        public bool Heartbeat
        {
            get { return heartbeat; }
            set { SetProperty(ref heartbeat, value); }
        }

        private string heartbeatMessage="无心跳";
        public string HeartbeatMessage
        {
            get { return heartbeatMessage; }
            set { SetProperty(ref heartbeatMessage, value); }
        }


        private string readSpeedRate="0次/s";
        public string ReadSpeedRate
        {
            get { return readSpeedRate; }
            set { SetProperty(ref readSpeedRate, value); }
        }
        private string writeSpeedRate="0次/s";
        public string WriteSpeedRate
        {
            get { return writeSpeedRate; }
            set { SetProperty(ref writeSpeedRate, value); }
        }

        private string readAndWriteSuccessRate="0%";
        public string ReadAndWriteSuccessRate
        {
            get { return readAndWriteSuccessRate; }
            set { SetProperty(ref readAndWriteSuccessRate, value); }
        }


        //private string selectedIpAddress;
        //public string SelectedIpAddress
        //{
        //    get { return selectedIpAddress; }
        //    set { SetProperty(ref selectedIpAddress, value); }
        //}


        private string portName="502";
        public string PortName
        {
            get { return portName; }
            set { SetProperty(ref portName, value); }
        }
        private int HeartbeatErrorCount = 0;


        public CommunicationConfigurationViewModel(IGetSystemDataService systemInfoService,IConfigurationService configurationService)
        {
           
            _systemInfoService=systemInfoService;
            _configurationService=configurationService;

            ConnectCommand = new DelegateCommand( ()=>
            {
                IsConnected= _configurationService.ModbusTcpConnect(IpName, int.Parse(PortName));
            }
            );
            DisconnectCommand = new DelegateCommand(() =>
            {
                IsConnected = _configurationService.ModbusTcpDisconnect();
            });



            Task.Run(async () =>
           {
               while (true)
               {
                    try
                    {
                        await Task.Delay(1000);

                        CpuUsage = _systemInfoService.GetCpu();
                        MemoryUsage = _systemInfoService.GetMemory();
                      
                   }
                   catch (Exception ex)
                   {
                       Debug.WriteLine(ex.Message);
                   }
               
               }
            
           });
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(200);
                        if (!IsConnected)
                            continue;
                        Heartbeat = _configurationService.ReadHearbeat();
                        HearbeatChecked();
                        _configurationService.WriteHearbeat();

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(1000);
                        if (IsConnected)
                            ReadSpeedRate = _configurationService.GetReadSpeed().ToString() + "次/s";
                            WriteSpeedRate = _configurationService.GetWriteSpeed().ToString() + "次/s";
                            ReadAndWriteSuccessRate = _configurationService.GetSuccessRate().ToString() + "%";

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            });
          
        }

        private void HearbeatChecked()
        {
            if (!Heartbeat)
            {
                HeartbeatErrorCount++;
                if (HeartbeatErrorCount >= 13)
                {
                    IsConnected = false;
                    HeartbeatMessage = "心跳异常";
                    throw new Exception("PLC Heartbeat Error");
                }
                else
                {
                    HeartbeatMessage = "心跳正常";
                }
            }
            else
            {
                HeartbeatErrorCount = 0;
                HeartbeatMessage = "心跳正常";
            }

        }

        //获取本机IP——保留不用
        //private void RefreshIpAddresses()
        //{
        //    ObservableCollection<string> newipAddresses = new ObservableCollection<string>();
        //    newipAddresses = _configurationService.GetIpAddress();
        //    foreach (var item in newipAddresses)
        //    {
        //        if (!IpAddresses.Contains(item))
        //        {
        //            IpAddresses.Add(item);
        //        }

        //        // 移除不存在的
        //        for (int i = IpAddresses.Count - 1; i >= 0; i--)
        //        {
        //            if (!newipAddresses.Contains(IpAddresses[i]))
        //                IpAddresses.RemoveAt(i);
        //        }
        //        // 处理 SelectedIp
        //        if (!string.IsNullOrEmpty(SelectedIpAddress))
        //        {
        //            // 如果当前选中的 IP 不在新集合里，切换到第一个
        //            if (!IpAddresses.Contains(SelectedIpAddress))
        //            {
        //                SelectedIpAddress = IpAddresses.FirstOrDefault();
        //            }
        //        }
        //        else
        //        {
        //            // 如果一开始就没选过，才赋第一个
        //            if (IpAddresses.Any())
        //                SelectedIpAddress = IpAddresses.First();
        //        }



        //    }
        //}



    }


}

