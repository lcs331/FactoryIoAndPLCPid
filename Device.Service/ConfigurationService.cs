using Device.IService;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Modbus.Device;
using System.Diagnostics.Metrics;
namespace Device.Service
{
    public class ConfigurationService : ModbusBase, IConfigurationService
    {
        public ObservableCollection<string> GetIpAddress()
        {
            ObservableCollection<string> ipAddress = new ObservableCollection<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress.Add(ip.ToString());
                }
            }

            return ipAddress;
        }
        public bool TryReconnect(string ip, int port)
        {
            if (!client.Connected)
            {
                ModbusTcpConnect(ip, port);
            }

            return true;
        }

        public bool ReadHearbeat()
        {
            short intValue = 0;
            if (master == null)
                return false;
            if (client.Connected)
            {               
                    byte slaveId = 1;         // PLC Modbus Server 的 ID
                    ushort startAddress = 14; // 对应 DB3.DBW26 的 Modbus 地址
                    ushort numRegisters = 1;  // Int 占 1 个寄存器（注意：S7 Int 是 16 位）              
                    ushort[] result = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
                    intValue = (short)result[0]; // 转换成有符号 Int16
                    if (intValue == 1)
                        RecordRead(true);
                    else
                        RecordRead(false);

               
                return intValue == 1;
            }
            else
                return false;
        }


        public bool WriteHearbeat()
        {
            try
            {
                if (master == null)
                    return false;
                if (client.Connected)
                {
                    byte slaveId = 1;         // PLC Modbus Server 的 ID
                    ushort startAddress = 13; // 对应 DB3.DBW26 的 Modbus 地址
                                              // Int 占 1 个寄存器（注意：S7 Int 是 16 位）
                   
                    Task.Run(async () =>
                    {

                        await Task.Delay(600);
                        master.WriteMultipleRegisters(slaveId, startAddress, new ushort[] { 1 });
                        RecordWrite(true);
                        await Task.Delay(600);
                        RecordWrite(true);
                        master.WriteMultipleRegisters(slaveId, startAddress, new ushort[] { 0 });
                    });

                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                throw ex;

            }

        }




        public int GetReadSpeed()
        {

            return Convert.ToInt32(GetReadRate());
        }
        public int GetWriteSpeed()
        {
            return Convert.ToInt32(GetWriteRate());
        }
        public int GetSuccessRate()
        {
            return Convert.ToInt32(GetTotalSuccessRate());
        }

    }
}
