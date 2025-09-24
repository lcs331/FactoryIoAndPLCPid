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
            try
            {
                client = new TcpClient(ip, port);
                master = ModbusIpMaster.CreateIp(client);
                byte slaveId = 1;         // PLC Modbus Server 的 ID
                ushort startAddress = 12; // 对应 DB3.DBW24 的 Modbus 地址
                ushort numRegisters = 1;  // Int 占 1 个寄存器（注意：S7 Int 是 16 位）

                ushort[] result = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                short intValue = (short)result[0]; // 转换成有符号 Int16
                if (intValue > 0)
                    RecordRead(true);
                else
                    RecordRead(false);
                return intValue > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
           
            
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

     
        public static float ConvertRegistersToFloat(ushort[] registers, bool swapWords = true)
        {
            if (registers == null || registers.Length < 2)
                throw new ArgumentException("需要至少2个寄存器来组成一个REAL数值。");

            byte[] bytes = new byte[4];

            if (swapWords)
            {
                // 默认：西门子 S7 常用字序
                bytes[0] = (byte)(registers[1] & 0xFF);     // 低字节
                bytes[1] = (byte)(registers[1] >> 8);       // 高字节
                bytes[2] = (byte)(registers[0] & 0xFF);
                bytes[3] = (byte)(registers[0] >> 8);
            }
            else
            {
                // 不交换：部分设备使用
                bytes[0] = (byte)(registers[0] & 0xFF);
                bytes[1] = (byte)(registers[0] >> 8);
                bytes[2] = (byte)(registers[1] & 0xFF);
                bytes[3] = (byte)(registers[1] >> 8);
            }

            return BitConverter.ToSingle(bytes, 0);
        }





    }
}
