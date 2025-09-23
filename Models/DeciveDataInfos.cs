using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DeciveDataInfos
    {
        public int DeviceStart {  get; set; }
        public int ErrorState { get; set; }

        public int StartInstruction { get; set; }
        public int StopInstruction{ get; set; }

        public int DeviceOnline { get; set; }
    }
}
