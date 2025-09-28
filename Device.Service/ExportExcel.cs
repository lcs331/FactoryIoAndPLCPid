using Device.IService;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Device.Service
{
    public class ExportExcel : IExportExcel
    {
        public async Task<bool> ExportToExcel<T>(string filePath, List<string> headers, List<T> data)
        {
            return await Task.Run(() =>
            { 
            try
            {
                IWorkbook workbook = new XSSFWorkbook(); // 创建 .xlsx 格式
                ISheet sheet = workbook.CreateSheet("Sheet1");

                // 样式：表头加粗居中
                ICellStyle headerStyle = workbook.CreateCellStyle();
                IFont headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);
                headerStyle.Alignment = HorizontalAlignment.Center;

                // 1️⃣ 写表头
                IRow headerRow = sheet.CreateRow(0);
                for (int i = 0; i < headers.Count; i++)
                {
                    ICell cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                // 获取泛型 T 的属性（用于反射取值）
                PropertyInfo[] props = typeof(T).GetProperties();

                // 2️⃣ 写数据
                for (int i = 0; i < data.Count; i++)
                {
                    IRow row = sheet.CreateRow(i + 1);
                    var item = data[i];

                    for (int j = 0; j < headers.Count && j < props.Length; j++)
                    {
                        object value = props[j].GetValue(item, null);
                        ICell cell = row.CreateCell(j);

                        if (value is DateTime dt)
                        {
                            cell.SetCellValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else if (value is int || value is double || value is float || value is decimal)
                        {
                            cell.SetCellValue(Convert.ToDouble(value));
                        }
                        else
                        {
                            cell.SetCellValue(value?.ToString() ?? "");
                        }
                    }
                }

                // 3️⃣ 自动调整列宽
                for (int i = 0; i < headers.Count; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                // 保存文件
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出 Excel 出错: {ex.Message}");
                return false;
            }
        });
      

        }
    }
}
