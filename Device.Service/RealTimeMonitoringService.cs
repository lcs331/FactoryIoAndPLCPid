using Device.IService;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Device.Service
{
    public class RealTimeMonitoringService : ModbusBase,IRealTimeMonitoringService
    {
        public DeciveDataInfos? GetDeciveDataInfos()
        {
            var data = new DeciveDataInfos();

            if (client!=null)
            {


                if (client == null || master == null || client.Connected == false)
                    return null;
                byte slaveId = 1;
                data.DeviceStart = master.ReadHoldingRegisters(slaveId, 0, 1)[0];
                data.WaterOutflowEate = ConvertRegistersToFloat(master.ReadHoldingRegisters(slaveId, 1, 2));
                data.EquipmentWaterLevel = ConvertRegistersToFloat(master.ReadHoldingRegisters(slaveId, 3, 2));
                data.ErrorState = master.ReadHoldingRegisters(slaveId, 5, 1)[0];
                data.StopInstruction = master.ReadHoldingRegisters(slaveId, 6, 1)[0];
                data.StartInstruction = master.ReadHoldingRegisters(slaveId, 7, 1)[0];
                data.WaterInletValve = ConvertRegistersToFloat(master.ReadHoldingRegisters(slaveId, 8, 2));
                data.WaterOutletValve = ConvertRegistersToFloat(master.ReadHoldingRegisters(slaveId, 10, 2));
                data.DeviceOnline = master.ReadHoldingRegisters(slaveId, 15, 1)[0];


                return data;
            }
            else
            {
                return data;
            }
            
        }
    }
}
