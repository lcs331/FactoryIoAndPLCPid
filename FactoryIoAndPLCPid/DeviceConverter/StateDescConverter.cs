using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FactoryIoAndPLCPid.DeviceConverter
{
    public class StateDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "未知";

            int state = System.Convert.ToInt32(value);

            switch (state)
            {
                case 0:
                    return "设备启动";
                case 1:
                    return "异常";
                case 2:
                    return "停止指示";
                case 3:
                    return "启动指示";
                case 4:
                    return "从站在线";
                default:
                    return "未知";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
