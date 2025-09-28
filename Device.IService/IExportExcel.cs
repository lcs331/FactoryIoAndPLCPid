using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.IService
{
    public interface IExportExcel
    {
        Task<bool> ExportToExcel<T>(string filePath, List<string> headers, List<T> data);

    }
}
