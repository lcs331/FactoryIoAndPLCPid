using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryIoAndPLCPid.Models
{
    public class PropertyCard
    {
        public string Title { get; set; }// 卡片标题，比如 "设备状态"
        public string Description { get; set; } // 卡片说明，比如 "设备是否启动"
        public object Value { get; set; }      // 属性值，比如 1 / 0 / 12.3
    }
}
