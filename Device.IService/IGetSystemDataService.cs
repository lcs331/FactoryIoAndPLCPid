using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.IService
{
    public interface IGetSystemDataService
    {
        double GetMemory();
        double GetCpu();
    }
}
