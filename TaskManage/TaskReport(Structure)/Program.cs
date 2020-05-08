using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using TaskManager.Model.Entities;
using TaskManager.Model.Context;
using System;

namespace TaskReport_Structure_
{
    class Program
    {
        static int row = 1;
        static int col = 1;
        static void Main(string[] args)
        {
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (TaskContext dbContext = new TaskContext())
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    List<TaskEntity> tasks = dbContext.Tasks.Include("ParentTask").Include("ChildTasks").ToList();
                    ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("Sheet1");
                    foreach (TaskEntity task in tasks)
                    {
                        WriteTaskRecursive(task, sheet);
                        row++;
                        col = 1;
                    }
                    if (!System.IO.Directory.Exists("Reports"))
                    {
                        System.IO.Directory.CreateDirectory("Reports");
                    }
                    string filePath = $"Reports\\{DateTime.Now.ToString("HHmm")}.xlsx";
                    excel.SaveAs(new System.IO.FileInfo(filePath));
                    
                }
            }
        }

        private static void WriteTaskRecursive(TaskEntity task, ExcelWorksheet sheet)
        {
            if (task.ParentTask != null)
            {
                WriteTaskRecursive(task.ParentTask, sheet);
            }

            sheet.Cells[row, col].Value = task.Name;
            col++;
        }
    }
}
