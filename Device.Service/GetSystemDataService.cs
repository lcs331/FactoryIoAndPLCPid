using Device.IService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Device.Service
{
    public class GetSystemDataService: IGetSystemDataService
    {

        public  GetSystemDataService()
        {
            GetCpu();
        }

        public double GetCpu()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            var load = searcher.Get()
                               .Cast<ManagementObject>()
                               .Sum(mo => Convert.ToDouble(mo["LoadPercentage"]));
            return load;
        }

        public double GetMemory()
        {

            var searcher = new ManagementObjectSearcher("Select FreePhysicalMemory,TotalVisibleMemorySize from Win32_OperatingSystem");

            foreach (var item in searcher.Get())
            {
                // 获取可用物理内存和总可见内存
                ulong freePhysicalMemory = (ulong)item["FreePhysicalMemory"];
                ulong totalVisibleMemorySize = (ulong)item["TotalVisibleMemorySize"];

                // 计算内存使用率
                double usedMemory = (double)(totalVisibleMemorySize - freePhysicalMemory) / totalVisibleMemorySize * 100;

                return usedMemory;

            }
            return 0;
        }
    }
}
