using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FactoryIoAndPLCPid.Models
{
    public class LineLegendItem
    {
        /// <summary>
        /// The name of the line
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The color of the line
        /// </summary>
        public Brush Color { get; set; } = Brushes.White;

    }

}
