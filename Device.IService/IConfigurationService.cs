using System.Collections.ObjectModel;

namespace Device.IService
{
    public interface IConfigurationService
    {
        //获取本机IP——不用
        ObservableCollection<string> GetIpAddress();

        bool ModbusTcpConnect(string ip, int port);

        bool ModbusTcpDisconnect();

        bool ReadHearbeat();

        bool WriteHearbeat();

        //获取读速率
        int GetReadSpeed();

        //获取写速率
        int GetWriteSpeed();

        //获取读写成功率
        int GetSuccessRate();

        //尝试重新连接
        bool TryReconnect();
    }
}
