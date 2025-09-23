using Device.IService;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Service
{
    public class RealTimeMonitoringService : ModbusBase,IRealTimeMonitoringService
    {
        public ObservableCollection<DeciveDataInfos> GetDeciveDataInfos()
        {
            ObservableCollection<DeciveDataInfos> deciveDataInfos = new ObservableCollection<DeciveDataInfos>();
            if (client.Connected)
            {

            }
            return deciveDataInfos;
        }
    }
}
