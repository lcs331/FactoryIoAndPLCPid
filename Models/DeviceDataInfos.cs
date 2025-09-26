using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{


    public class DeviceDataInfos
    {
        public int DeviceStart {  get; set; }
        public int ErrorState { get; set; }
        /// <summary>
        /// 设备水位
        /// </summary>
        public float EquipmentWaterLevel { get; set; }
        /// <summary>
        /// 出水速率
        /// </summary>
        public float WaterOutflowRate {  get; set; }
        public int StartInstruction { get; set; }
        public int StopInstruction{ get; set; }

        /// <summary>
        /// 进水阀门
        /// </summary>
        public float WaterInletValve { get; set; }
        /// <summary>
        /// 出水阀门
        /// </summary>
        public float WaterOutletValve { get; set; }

        public int DeviceOnline { get; set; }

        public DateTime DeviceDateTime { get; set; }
    }
}
