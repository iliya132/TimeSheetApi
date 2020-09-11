﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using TimeSheetApi.Model;
using TimeSheetApi.Model.Entities;

namespace TimeSheetApp.Model.Reports
{
    /// <summary>
    /// Отчет о изменениях в распределении сотрудников по процессам
    /// </summary>
    public class Report_03 : IReport
    {
        private readonly List<StructuredAnalytic> analyticsOrdered;
        private readonly TimeSheetContext dataBase;

        public Report_03(IEnumerable<Analytic> analytics, TimeSheetContext _dataBase)
        {
            analyticsOrdered = new List<StructuredAnalytic>();
            foreach (Analytic analytic in analytics)
            {
                analyticsOrdered.Add(new StructuredAnalytic(analytic));
            }
            dataBase = _dataBase;
        }

        public FileInfo Generate(DateTime start, DateTime end)
        {
            List<StructureData> structuresData = new List<StructureData>();
            List<Process> _processes = dataBase.ProcessSet.ToList();
            List<TimeSheetTable> TimeSheetTableDB = dataBase.TimeSheetTableSet.Where(i => i.TimeStart >= start &&
                i.TimeStart <= end).
                ToList();
            List<Analytic> analytics = TimeSheetTableDB.Select(i=>i.Analytic).Distinct().ToList();

            #region Создание списка подразделений

            foreach (StructuredAnalytic analytic in analyticsOrdered)
            {
                if (analytic.Analytic.FirstName.Equals("null") || analytic.Analytic.FirstName.Equals("NotSet"))
                    continue;
                string structure;

                if (analytic.FourStructure == null)
                {
                    if (analytic.ThirdStructure == null)
                    {
                        structure = analytic.SecondStructure;
                    }
                    else
                    {
                        structure = analytic.ThirdStructure;
                    }
                }
                else
                {
                    structure = analytic.FourStructure;
                }

                if (!structuresData.Any(i => i.structName.Equals(structure)))
                {
                    structuresData.Add(new StructureData() { structName = structure });
                }
                structuresData.First(i => i.structName.Equals(structure)).analytics.Add(analytic);
            }

            #endregion

            #region Для каждого процесса


            foreach (Process process in _processes)
            {
                
                #region Для каждого подразделения считаем значение
                if (process.Id != 62 && process.Id != 63)
                {
                    foreach (StructureData structure in structuresData)
                    {
                        double operationPercentTotal = 0.0;
                        int timeSpent = 0; //Время потраченное в рамках одного процесса
                        int timeSpentTotal = 0; //Время потраченное на все процессы

                        foreach (StructuredAnalytic analytic in structure.analytics)
                        {
                            if (TimeSheetTableDB.Any(i => i.AnalyticId == analytic.Analytic.Id &&
                            i.TimeStart > start &&
                            i.TimeStart < end))
                            {
                                timeSpentTotal = TimeSheetTableDB.
                                    Where(i => i.AnalyticId == analytic.Analytic.Id &&
                                        i.TimeStart > start &&
                                        i.TimeStart < end &&
                                        i.Process_id != 62 &&
                                        i.Process_id != 63).
                                    Sum(i => i.TimeSpent);
                            }

                            if (TimeSheetTableDB.Any(i => i.AnalyticId == analytic.Analytic.Id &&
                            i.Process_id == process.Id &&
                            i.TimeStart > start &&
                            i.TimeStart < end))
                            {
                                timeSpent = TimeSheetTableDB.
                                    Where(i => i.AnalyticId == analytic.Analytic.Id &&
                                    i.Process_id == process.Id &&
                                    i.TimeStart > start &&
                                    i.TimeStart < end).
                                    Sum(i => i.TimeSpent);
                            }
                            else { timeSpent = 0; }
                            if (timeSpentTotal != 0)
                            {
                                operationPercentTotal += timeSpent * 1.00 / timeSpentTotal;

                            }
                        }
                        if (operationPercentTotal > 0)
                            structure.processValues.Add(process.ProcName, operationPercentTotal);
                    }
                }
                #endregion
            }


            #endregion

            #region итог записываем в excel
            using (ExcelPackage excel = new ExcelPackage())
            {
                ExcelWorksheet sheet = excel.Workbook.Worksheets.Add("Report_03");
                int row = 2;

                sheet.Cells[1, 1].Value = "Процесс";
                sheet.Cells[1, 2].Value = "Код процесса";

                foreach (Process process in _processes)
                {
                    sheet.Cells[row, 1].Value = process.ProcName;
                    sheet.Cells[row++, 2].Value = $"{process.Block_Id}.{process.SubBlock_Id}.{process.Id}";
                }
                for (int i = 0; i < structuresData.Count; i++)
                {
                    sheet.Cells[1,i+3].Value = structuresData[i].structName;
                }

                for (int i = 2; i <= _processes.Count; i++)
                {
                    string procName = sheet.Cells[i, 1].Text;
                    
                    for (int j = 0; j < structuresData.Count; j++)
                    {

                        if (structuresData[j].processValues.ContainsKey(procName))
                        {
                            sheet.Cells[i, j + 3].Value = structuresData[j].processValues[procName];
                        }
                    }
                }


                string fileName = $"Reports\\Report_03_{Environment.UserName}{DateTime.Now.ToString($"ddMMyyyy_HHmmss")}.xlsx";
                FileInfo newExcelFile = new FileInfo(fileName);
                if (!Directory.Exists("Reports\\"))
                {
                    Directory.CreateDirectory("Reports\\");
                }
                excel.SaveAs(newExcelFile);
                excel.Dispose();
                return new FileInfo(fileName);
            }
            #endregion



        }
        internal class StructureData
        {
            internal string structName { get; set; }
            internal Dictionary<string, double> processValues = new Dictionary<string, double>();
            internal List<StructuredAnalytic> analytics = new List<StructuredAnalytic>();
        }

    }
}
