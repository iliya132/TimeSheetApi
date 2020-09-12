using Microsoft.EntityFrameworkCore;

using OfficeOpenXml;
using OfficeOpenXml.Style;

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TimeSheetApi.Model.Entities;

using TimeSheetApp.Model.Reports;

namespace TimeSheetApi.Model.Reports
{
    public class AnalyticActivityReport_01 : IReport
    {
        public AnalyticActivityReport_01(TimeSheetContext _dbContext, IEnumerable<Analytic> analytics) : base(_dbContext, analytics)
        {
        }

        public override FileInfo Generate(DateTime start, DateTime end)
        {
            return ExportDataTableToExcel(GetAnalyticsData(start, end));
        }

        private DataTable GetAnalyticsData(DateTime TimeStart, DateTime TimeEnd)
        {
            DataTable dataTable = new DataTable();

            #region placeColumns
            dataTable.Columns.Add("LastName");
            dataTable.Columns.Add("FirstName");
            dataTable.Columns.Add("FatherName");
            dataTable.Columns.Add("BlockName");
            dataTable.Columns.Add("SubblockName");
            dataTable.Columns.Add("ProcessName");
            dataTable.Columns.Add("Subject");
            dataTable.Columns.Add("Body");
            dataTable.Columns.Add("TimeStart");
            dataTable.Columns.Add("TimeEnd");
            dataTable.Columns.Add("TimeSpent");
            dataTable.Columns.Add("BusinessBlockName");
            dataTable.Columns.Add("SupportsName");
            dataTable.Columns.Add("ClientWaysName");
            dataTable.Columns.Add("EscalationsName");
            dataTable.Columns.Add("FormatsName");
            dataTable.Columns.Add("RiskName");
            #endregion

            #region getData
            foreach (Analytic analytic in analytics)
            {
                List<TimeSheetTable> ReportEntity = new List<TimeSheetTable>();
                ReportEntity = _dbContext.TimeSheetTableSet.Include(i=>i.BusinessBlocks).ThenInclude(i=>i.BusinessBlock).
                    Include(i=>i.Supports).ThenInclude(i=>i.Supports).Include(i=>i.Risks).ThenInclude(i=>i.Risk).Include(i=>i.Escalations).ThenInclude(i=>i.Escalation)
                    .Include(i => i.Process).ThenInclude(i=>i.Block).Include(i=>i.Process.SubBlock).
                    Include(i=>i.Analytic).Include(i=>i.ClientWays).Include(i=>i.Formats).
                    Where(record => record.AnalyticId == analytic.Id &&
                    record.TimeStart > TimeStart && record.TimeStart < TimeEnd).ToList();
                for (int i = 0; i < ReportEntity.Count; i++)
                {
                    DataRow row = dataTable.Rows.Add();
                    row["LastName"] = ReportEntity[i].Analytic.LastName;
                    row["FirstName"] = ReportEntity[i].Analytic.FirstName;
                    row["FatherName"] = ReportEntity[i].Analytic.FatherName;
                    row["BlockName"] = ReportEntity[i].Process.Block.BlockName;
                    row["SubblockName"] = ReportEntity[i].Process.SubBlock.SubblockName;
                    row["ProcessName"] = ReportEntity[i].Process.ProcName;
                    row["Subject"] = ReportEntity[i].Subject;
                    row["Body"] = ReportEntity[i].Comment;
                    row["TimeStart"] = ReportEntity[i].TimeStart;
                    row["TimeEnd"] = ReportEntity[i].TimeEnd;
                    row["TimeSpent"] = ReportEntity[i].TimeSpent;
                    #region Добавление информации о мультивыборе
                    StringBuilder choice = new StringBuilder();
                    foreach (BusinessBlockNew item in ReportEntity[i].BusinessBlocks)
                    {
                        choice.Append($"{item.BusinessBlock.BusinessBlockName}; ");
                    }
                    row["BusinessBlockName"] = choice.ToString();
                    #endregion
                    #region Добавление строки о мультивыборе саппорта
                    choice.Clear();
                    foreach (SupportNew item in ReportEntity[i].Supports)
                    {
                        choice.Append($"{item.Supports.Name}; ");
                    }
                    row["SupportsName"] = choice.ToString();
                    #endregion
                    row["ClientWaysName"] = ReportEntity[i].ClientWays.Name;
                    #region добавление строки о мультивыбора эскалации
                    choice.Clear();
                    foreach (EscalationNew item in ReportEntity[i].Escalations)
                    {
                        choice.Append($"{item.Escalation.Name}; ");
                    }
                    row["EscalationsName"] = choice.ToString();
                    #endregion
                    row["FormatsName"] = ReportEntity[i].Formats.Name;
                    #region добавление строки о мультивыборе риска
                    choice.Clear();
                    foreach (RiskNew item in ReportEntity[i].Risks)
                    {
                        choice.Append($"{item.Risk.RiskName}; ");
                    }
                    row["RiskName"] = choice.ToString();
                    #endregion
                }
            }
            #endregion
            return dataTable;
        }

        private FileInfo ExportDataTableToExcel(DataTable data)
        {
            #region EPPLUS
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Sheet1");
            worksheet.Cells[2, 1].LoadFromDataTable(data, false);
            worksheet.Cells[1, 1].Value = "Фамилия";
            worksheet.Cells[1, 2].Value = "Имя";
            worksheet.Cells[1, 3].Value = "Отчество";
            worksheet.Cells[1, 4].Value = "Блок";
            worksheet.Cells[1, 5].Value = "Подблок";
            worksheet.Cells[1, 6].Value = "Процесс";
            worksheet.Cells[1, 7].Value = "Тема";
            worksheet.Cells[1, 8].Value = "Комментарий";
            worksheet.Cells[1, 9].Value = "Начало";
            worksheet.Cells[1, 10].Value = "Окончание";
            worksheet.Cells[1, 11].Value = "Длительность";
            worksheet.Cells[1, 12].Value = "Бизнес подразделение";
            worksheet.Cells[1, 13].Value = "Саппорт";
            worksheet.Cells[1, 14].Value = "Клиентский путь";
            worksheet.Cells[1, 15].Value = "Эскалация";
            worksheet.Cells[1, 16].Value = "Формат";
            worksheet.Cells[1, 17].Value = "Риск";
            worksheet.Column(11).Style.Numberformat.Format = "0";
            worksheet.Column(9).Style.Numberformat.Format = @"dd.MM.yyyy hh:mm";
            worksheet.Column(10).Style.Numberformat.Format = @"dd.MM.yyyy hh:mm";
            worksheet.Column(1).Width = 35;
            worksheet.Column(4).Width = 35;
            worksheet.Column(5).Width = 35;
            worksheet.Column(6).Width = 35;
            worksheet.Column(7).Width = 30;
            worksheet.Column(8).Width = 30;
            worksheet.Column(9).Width = 15.5;
            worksheet.Column(10).Width = 15.5;
            worksheet.Column(11).Width = 13.5;
            worksheet.Column(12).Width = 25;
            worksheet.Column(13).Width = 25;
            worksheet.Column(14).Width = 25;
            worksheet.Column(15).Width = 25;
            worksheet.Column(16).Width = 25;
            worksheet.Column(17).Width = 25;
            for (int i = 2; i < worksheet.Dimension.Rows + 1; i++)
                worksheet.Cells[i, 1].Value = $"{worksheet.Cells[i, 1].Value} {worksheet.Cells[i, 2].Value} {worksheet.Cells[i, 3].Value}";
            worksheet.DeleteColumn(2);
            worksheet.DeleteColumn(2);
            worksheet.Row(2).OutlineLevel = 1;
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            string analyticName = worksheet.Cells[2, 1].Text;
            int analyticStartRow = 2;
            int RowsCount = worksheet.Dimension.Rows + 1;
            for (int i = 3; i < RowsCount + 1; i++)
            {
                worksheet.Row(i - 1).Collapsed = true;
                if (!string.IsNullOrWhiteSpace(analyticName) && analyticName == worksheet.Cells[i, 1].Text)
                {
                    worksheet.Row(i).OutlineLevel = 2;

                }
                else if (!string.IsNullOrWhiteSpace(analyticName))
                {
                    worksheet.InsertRow(i, 1);

                    worksheet.Cells[i, 1, i, 15].Style.Font.Bold = true;
                    worksheet.Cells[i, 1, i, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[i, 1, i, 15].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    worksheet.Cells[i, 9].Formula = $"SUM(I{analyticStartRow}:I{i - 1})";
                    worksheet.Cells[i, 1].Value = $"ИТОГО: {worksheet.Cells[i - 1, 1].Text}";
                    worksheet.Row(i).OutlineLevel = 0;
                    i++;
                    RowsCount++;
                    analyticStartRow = i;
                    if (string.IsNullOrWhiteSpace((analyticName = worksheet.Cells[i, 1].Text)))
                        break;
                    worksheet.Row(i).OutlineLevel = 2;
                }
            }

            string fileName = $"Reports\\Report{Environment.UserName}{DateTime.Now.ToString($"ddMMyyyy_HHmmss")}.xlsx";
            FileInfo newExcelFile = new FileInfo(fileName);
            if (!Directory.Exists("Reports\\"))
            {
                Directory.CreateDirectory("Reports\\");
            }
            excel.SaveAs(newExcelFile);
            excel.Dispose();
            #endregion
            return new FileInfo(fileName);
        }

    }
}
