using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LogMessage
    {
        public DateTime Time { get; set; }
        public string Level { get; set; }        // 比如 "INFO", "WARN", "ERROR"
        public string Source { get; set; }       // 来源模块
        public string Message { get; set; }      // 内容
        public int RepeatCount { get; set; } = 1;
        public DateTime LastTime { get; set; }


    }
}
