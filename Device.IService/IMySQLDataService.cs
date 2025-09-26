using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.IService
{
    public interface IMySQLDataService
    {
        // 增加设备数据记录
        Task InsertDeviceData(DeviceDataInfos data);

        // 查询设备数据记录
        Task<IEnumerable<DeviceDataInfos>> GetDeviceDataHistory();

        // 更新设备数据记录
        Task UpdateDeviceData(DeviceDataInfos data);

        // 删除设备数据记录
        Task DeleteDeviceData(int id);

    }
}
