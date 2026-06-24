using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;

using THP.Extention.Constants;
using THP.Extention.Models;

namespace THP.Extention.Services
{
    public class ExcelExportService
    {
        public void Export(BugHistoryItem item)
        {
            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet sheet = null;

            try
            {
                Directory.CreateDirectory(ReportBugPaths.RootFolder);

                app = new Excel.Application();

                if (File.Exists(ReportBugPaths.ExcelFile))
                {
                    workbook = app.Workbooks.Open(ReportBugPaths.ExcelFile);
                }
                else
                {
                    workbook = app.Workbooks.Add();

                    sheet = (Excel.Worksheet)workbook.Sheets[1];

                    sheet.Cells[1, 1] = "ลำดับ";
                    sheet.Cells[1, 2] = "วัน/เวลา";
                    sheet.Cells[1, 3] = "สถานะ";
                    sheet.Cells[1, 4] = "รายละเอียด";
                    sheet.Cells[1, 5] = "รูปภาพ";

                    workbook.SaveAs(ReportBugPaths.ExcelFile);
                }

                sheet = (Excel.Worksheet)workbook.Sheets[1];

                int row = 2;

                while (sheet.Cells[row, 1].Value2 != null)
                {
                    row++;
                }

                sheet.Cells[row, 1] = item.No;
                sheet.Cells[row, 2] = item.Date;
                sheet.Cells[row, 3] = item.Status;
                sheet.Cells[row, 4] = item.Description;
                sheet.Cells[row, 5] = item.ScreenshotPath;

                sheet.Hyperlinks.Add(
                    sheet.Cells[row, 5],
                    item.ScreenshotPath,
                    Type.Missing,
                    "เปิดรูปภาพ",
                    "เปิดรูป");

                workbook.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Excel Error");
            }
            finally
            {
                if (sheet != null)
                {
                    Marshal.ReleaseComObject(sheet);
                    sheet = null;
                }

                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                    workbook = null;
                }

                if (app != null)
                {
                    app.Quit();
                    Marshal.ReleaseComObject(app);
                    app = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}