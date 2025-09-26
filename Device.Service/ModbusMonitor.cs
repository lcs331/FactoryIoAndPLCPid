using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Service
{
    public class ModbusMonitor
    {

   
        private long totalReadCount = 0;       // 总读次数
        private long successReadCount = 0;     // 成功读次数
        private long totalWriteCount = 0;      // 总写次数
        private long successWriteCount = 0;    // 成功写次数

        private Stopwatch readStopwatch = new Stopwatch();
        private Stopwatch writeStopwatch = new Stopwatch();

        public ModbusMonitor()
        {
            readStopwatch.Start();
            writeStopwatch.Start();
        }

        // 记录一次读请求
     
        public void RecordRead(bool isSuccess)
        {
         
            
            Interlocked.Increment(ref totalReadCount);
            if (isSuccess) Interlocked.Increment(ref successReadCount);
            if(successReadCount>5000)
                successReadCount=0;
            
        }

        // 记录一次写请求
        public void RecordWrite(bool isSuccess)
        {
                 
            Interlocked.Increment(ref totalWriteCount);
            if (isSuccess) Interlocked.Increment(ref successWriteCount);
            if (successWriteCount > 5000)
                successWriteCount = 0; 
            
        }

        // 读速率（次/秒）
        public double GetReadRate()
        {
            double seconds = readStopwatch.Elapsed.TotalSeconds;
            if (seconds <= 0) return 0;
            return Math.Round(totalReadCount / seconds, 2);
        }

        /// <summary>
        /// /写速率（次/秒）
        /// </summary>
        /// <returns></returns>
        public double GetWriteRate()
        {
            double seconds = writeStopwatch.Elapsed.TotalSeconds;
            if (seconds <= 0) return 0;
            return Math.Round(totalWriteCount / seconds, 2);
        }

        //总成功率（百分比）
        public double GetTotalSuccessRate()
        {
            if (totalReadCount == 0 && totalWriteCount == 0) return 0;
            //最大1000
            if (totalReadCount  > 10000) totalReadCount = 0;
            if (totalWriteCount > 10000) totalWriteCount = 0;
            return Math.Round((double)(successReadCount + successWriteCount) / (totalReadCount + totalWriteCount) * 100, 2);
        }
        
    }
}
