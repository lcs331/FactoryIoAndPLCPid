using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.Models
{
    public class PropertyLiveChats
    {

        public DateTime Timestamp { get; set; }   // 时间
        public float WaterLevel { get; set; }     // 水位
        public float OutflowRate { get; set; }    // 出水速率
        public float InletValve { get; set; }     // 进水阀门
        public float OutletValve { get; set; }    // 出水阀门

    }
}
