using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Device.Service
{
    public class ModbusBase: ModbusMonitor
    {
        public static TcpClient client;
        public static ModbusIpMaster master;


        public bool ModbusTcpConnect(string ip, int port)
        {

            client = new TcpClient(ip, port);
            master = ModbusIpMaster.CreateIp(client);
            byte slaveId = 1;         // PLC Modbus Server 的 ID
            ushort startAddress = 12; // 对应 DB3.DBW24 的 Modbus 地址
            ushort numRegisters = 1;  // Int 占 1 个寄存器（注意：S7 Int 是 16 位）
            
            ushort[] result = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
            
            short intValue = (short)result[0]; // 转换成有符号 Int16
            if(intValue > 0)
            RecordRead(true);
            else
            RecordRead(false);
            return intValue > 0;
            
        }

        public bool ModbusTcpDisconnect()
        {
            if (client.Connected)
            {
                client.Close();
                return true;
            }
            else
            {   
                return false;
            }
        }





     
    }
}
