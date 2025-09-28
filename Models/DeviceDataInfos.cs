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
        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceStart {  get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        public int ErrorState { get; set; }
        /// <summary>
        /// 设备水位
        /// </summary>
        public float EquipmentWaterLevel { get; set; }
        /// <summary>
        /// 出水速率
        /// </summary>
        public float WaterOutflowRate {  get; set; }
        /// <summary>
        /// 进水速率
        /// </summary>
        public int StartInstruction { get; set; }
        /// <summary>
        /// 停止指令
        /// </summary>
        public int StopInstruction{ get; set; }

        /// <summary>
        /// 进水阀门
        /// </summary>
        public float WaterInletValve { get; set; }
        /// <summary>
        /// 出水阀门
        /// </summary>
        public float WaterOutletValve { get; set; }
        /// <summary>
        /// 设备在线
        /// </summary>
        public int DeviceOnline { get; set; }
        /// <summary>
        /// 设备时间
        /// </summary>
        
        public DateTime DeviceDateTime { get; set; }
    }
}
