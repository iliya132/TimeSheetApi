﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Windows;
using TimeSheetApi.Model.Entities;
using System.Text;
using TimeSheetApi.Model.Interfaces;
using System.Diagnostics;
using Process = TimeSheetApi.Model.Entities.Process;

namespace TimeSheetApi.Model.Implementations
{
    class EFDataProvider : IDataProvider
    {
        TimeSheetContext _dbContext = new TimeSheetContext();
        Analytic _currentAnalytic;
        const int DEPARTMENT_HEAD = 1;
        const int DIRECTION_HEAD = 2;
        const int UPRAVLENIE_HEAD = 3;
        const int OTDEL_HEAD = 4;
        const int ADMIN = 5;
        const int USER = 6;

        /// <summary>
        /// Получить подсказки для поля "Тема"
        /// </summary>
        /// <param name="process">Процесс, к которому нужно подобрать подсказки</param>
        /// <returns>Стек тем, введенных ранее пользователем</returns>
        public IEnumerable<string> GetSubjectHints(int Process_id, string userName)
        {
            Process process = _dbContext.ProcessSet.FirstOrDefault(i => i.Id == Process_id);
            if (process != null)
            {
                return _dbContext.TimeSheetTableSet.
                    Where(i => i.Analytic.UserName.ToLower().Equals(userName.ToLower()) &&
                        i.Subject.Length > 0 &&
                        i.Process_id == process.Id).OrderBy(i => i.TimeStart).Select(i => i.Subject).ToList();
            }
            return new List<string>();
        }

        /// <summary>
        /// Получить список всех существующих процессов
        /// </summary>
        /// <returns>ObservableCollection</returns>
        public IEnumerable<Process> GetProcesses() => new List<Process>(_dbContext.ProcessSet.Include(i=>i.Block).Include(i=>i.SubBlock));

        /// <summary>
        /// Получить список всех БизнесПодразделений
        /// </summary>
        /// <returns>OBservableCollection</returns>
        public IEnumerable<BusinessBlock> GetBusinessBlocks() => _dbContext.BusinessBlockSet.ToList();

        /// Добавить запись в TimeSheetTable
        /// </summary>
        /// <param name="activity">Процесс</param>
        public void AddActivity(TimeSheetTable newRecord)
        {
            newRecord.TimeSpent = (int)(newRecord.TimeEnd - newRecord.TimeStart).TotalMinutes;
            newRecord.AnalyticId = newRecord.Analytic != null ? newRecord.Analytic.Id : newRecord.AnalyticId;
            newRecord.Analytic = null;
            _dbContext.TimeSheetTableSet.Add(newRecord);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Удалить запись из БД
        /// </summary>
        /// <param name="record"></param>
        public void DeleteRecord(int record_id)
        {
            TimeSheetTable record = _dbContext.TimeSheetTableSet
                .Include("BusinessBlocks")
                .Include("Supports")
                .Include("Escalations")
                .Include("Risks")
                .FirstOrDefault(i => i.Id == record_id);
            if (record.BusinessBlocks != null && record.BusinessBlocks.Count > 0)
            {
                while (record.BusinessBlocks.Count > 0)
                {
                    int id = record.BusinessBlocks[0].Id;
                    record.BusinessBlocks.RemoveAt(0);
                    _dbContext.NewBusinessBlockSet.Remove(_dbContext.NewBusinessBlockSet.First(o => o.Id == id));
                }
            }
            if (record.Supports != null && record.Supports.Count > 0)
            {
                while (record.BusinessBlocks.Count > 0)
                {
                    int id = record.Supports[0].Id;
                    record.Supports.RemoveAt(0);
                    _dbContext.NewSupportsSet.Remove(_dbContext.NewSupportsSet.First(o => o.Id == id));
                }
            }
            if (record.Escalations != null && record.Escalations.Count > 0)
            {
                while (record.Escalations.Count > 0)
                {
                    int id = record.Escalations[0].Id;
                    record.Escalations.RemoveAt(0);
                    _dbContext.NewEscalations.Remove(_dbContext.NewEscalations.First(o => o.Id == id));
                }
            }
            if (record.Risks != null && record.Risks.Count > 0)
            {
                while (record.Risks.Count > 0)
                {
                    int id = record.Risks[0].Id;
                    record.Risks.RemoveAt(0);
                    _dbContext.NewRiskSet.Remove(_dbContext.NewRiskSet.First(o => o.Id == id));
                }
            }
            _dbContext.TimeSheetTableSet.Remove(record);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Отслеживание состояния принудительного выхода
        /// </summary>
        /// <returns>false</returns>
        public bool ForcedToQuit()
        {
            return false;
        }

        /// <summary>
        /// Возвращает названия всех блоков
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetProcessBlocks() => _dbContext.BlockSet.Select(i => i.BlockName).ToList();

        /// <summary>
        /// Получить список всех клиентских путей
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ClientWays> GetClientWays() => _dbContext.ClientWaysSet.ToList();

        /// <summary>
        /// Получить список всех Эскалаций
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Escalation> GetEscalation() => _dbContext.EscalationsSet.ToList();

        /// <summary>
        /// Получить список всех форматов
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Formats> GetFormat() => _dbContext.FormatsSet.ToList();

        /// <summary>
        /// Получить список сотрудников в подчинении
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IEnumerable<Analytic> GetMyAnalyticsData(string userName)
        {
            Analytic currentUser = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName.ToLower()));
            List<Analytic> analytics = new List<Analytic>();
            switch (currentUser.RoleTableId)
            {
                case (DEPARTMENT_HEAD):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Where(i => i.DepartmentId == currentUser.DepartmentId).Include(i=>i.Departments).Include(i=>i.Directions).Include(i=>i.Upravlenie).Include(i=>i.Otdel).ToArray());
                    break;
                case (DIRECTION_HEAD):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Where(i => i.DirectionId == currentUser.DirectionId).Include(i => i.Departments).Include(i => i.Directions).Include(i => i.Upravlenie).Include(i => i.Otdel));
                    break;
                case (UPRAVLENIE_HEAD):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Where(i => i.UpravlenieId == currentUser.UpravlenieId).Include(i => i.Departments).Include(i => i.Directions).Include(i => i.Upravlenie).Include(i => i.Otdel));
                    break;
                case (OTDEL_HEAD):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Where(i => i.OtdelId == currentUser.OtdelId).Include(i => i.Departments).Include(i => i.Directions).Include(i => i.Upravlenie).Include(i => i.Otdel).ToArray());
                    break;
                case (ADMIN):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Include(i => i.Departments).Include(i => i.Directions).Include(i => i.Upravlenie).Include(i => i.Otdel).ToArray());
                    break;
                case (USER):
                    analytics = new List<Analytic>(_dbContext.AnalyticSet.Where(i => i.Id == currentUser.Id || i.HeadFuncId == currentUser.Id).Include(i => i.Departments).Include(i => i.Directions).Include(i => i.Upravlenie).Include(i => i.Otdel).ToArray());
                    break;
            }
            return analytics;
        }

        /// <summary>
        /// Выгрузить отчет
        /// </summary>
        /// <param name="ReportType">Тип отчета</param>
        /// <param name="analytics">список выбранных аналитиков</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void GetReport(int ReportType, Analytic[] analytics, DateTime start, DateTime end)
        {

        }

        /// <summary>
        /// Получить список всех рисков
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Risk> GetRisks() => _dbContext.RiskSet.ToList();

        /// <summary>
        /// Получить список названий подблоков
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSubBlocksNames() => _dbContext.SubBlockSet.Select(i => i.SubblockName).ToList();

        /// <summary>
        /// Получить массив всех саппортов
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Supports> GetSupports() => _dbContext.SupportsSet.ToList();

        /// <summary>
        /// Метод устанавливает свойство видимости вкладки "Кабинет руководителя"
        /// </summary>
        /// <param name="currentUser">Текущий пользователь</param>
        /// <returns></returns>
        public bool IsAnalyticHasAccess(string userName) => _dbContext.AnalyticSet.
            FirstOrDefault(a => a.UserName.ToLower().Equals(userName.ToLower())).Role.Id < 6 ?
            true :
            false;

        /// <summary>
        /// Получает информацию о текущем аналитике, и если запись в БД не существует - создаёт новую
        /// </summary>
        /// <returns></returns>
        public Analytic LoadAnalyticData(string userName)
        {
            Analytic analytic;
            analytic = _dbContext.AnalyticSet.Include("Departments").Include("Directions").Include("Upravlenie").Include("Otdel").Include("AdminHead").Include("FunctionHead").FirstOrDefault(i => i.UserName.ToLower().Equals(userName));
            if (analytic == null)
            {
                analytic = new Analytic()
                {
                    //TODO доработать загрузку данных из БД Oracle
                    UserName = userName,
                    DepartmentId = 1,
                    DirectionId = 1,
                    FirstName = "NotSet",
                    LastName = "NotSet",
                    FatherName = "NotSet",
                    OtdelId = 1,
                    PositionsId = 1,
                    RoleTableId = 1,
                    UpravlenieId = 1
                };
                _dbContext.AnalyticSet.Add(analytic);
                _dbContext.SaveChanges();
                analytic = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName));
            }
            _currentAnalytic = analytic;
            return analytic;
        }

        /// <summary>
        /// Загружает все записи в TimeSheetTable по аналитику за выбранную дату
        /// </summary>
        /// <param name="date"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<TimeSheetTable> LoadTimeSheetRecords(DateTime date, string userName)
        {
            Analytic user = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName.ToLower()));
            List<TimeSheetTable> timeSheetTables;
            timeSheetTables = _dbContext.TimeSheetTableSet.
                Include(i => i.BusinessBlocks).ThenInclude(i => i.BusinessBlock).
                Include(i => i.Risks).ThenInclude(i => i.Risk).
                Include(i => i.Escalations).ThenInclude(i => i.Escalation).
                Include(i => i.Supports).ThenInclude(i => i.Supports).
                Include(i=>i.Process).
                Include(i=>i.ClientWays).Include(i=>i.Formats).
                Where(i => i.AnalyticId == user.Id && i.TimeStart.Date == date.Date).ToList();
            return timeSheetTables;
        }

        /// <summary>
        /// Изменить процесс
        /// </summary>
        /// <param name="oldProcess"></param>
        /// <param name="newRecord"></param>
        public void UpdateProcess(int oldRecordId, TimeSheetTable newRecord)
        {
            TimeSheetTable oldRecord = _dbContext.TimeSheetTableSet.FirstOrDefault(i => i.Id == oldRecordId);
            oldRecord.Process_id = newRecord.Process_id;
            oldRecord.Subject = newRecord.Subject;
            oldRecord.TimeStart = newRecord.TimeStart;
            oldRecord.TimeEnd = newRecord.TimeEnd;
            oldRecord.Comment = newRecord.Comment;
            oldRecord.TimeSpent = (int)(oldRecord.TimeEnd - oldRecord.TimeStart).TotalMinutes;
            oldRecord.ClientWaysId = newRecord.ClientWaysId;
            oldRecord.FormatsId = newRecord.FormatsId;
            _dbContext.NewBusinessBlockSet.RemoveRange(_dbContext.NewBusinessBlockSet.Where(i => i.TimeSheetTableId == oldRecord.Id));
            _dbContext.NewEscalations.RemoveRange(_dbContext.NewEscalations.Where(i => i.TimeSheetTableId == oldRecord.Id));
            _dbContext.NewRiskSet.RemoveRange(_dbContext.NewRiskSet.Where(i => i.TimeSheetTableId == oldRecord.Id));
            _dbContext.NewSupportsSet.RemoveRange(_dbContext.NewSupportsSet.Where(i => i.TimeSheetTableId == oldRecord.Id));
            oldRecord.Supports = new List<SupportNew>();
            oldRecord.Escalations = new List<EscalationNew>();
            oldRecord.Risks = new List<RiskNew>();
            oldRecord.BusinessBlocks = new List<BusinessBlockNew>();
            oldRecord.Supports.AddRange(newRecord.Supports);
            oldRecord.Escalations.AddRange(newRecord.Escalations);
            oldRecord.Risks.AddRange(newRecord.Risks);
            oldRecord.BusinessBlocks.AddRange(newRecord.BusinessBlocks);
            //TODO Перекидывать множественный выбор
            _dbContext.SaveChanges();
        }

        public void RemoveSelection(int record_id)
        {
            TimeSheetTable record = _dbContext.TimeSheetTableSet.FirstOrDefault(i => i.Id == record_id);
            List<BusinessBlockNew> businessBlocksToDelete = _dbContext.NewBusinessBlockSet.Where(rec => rec.TimeSheetTableId == record.Id).ToList();
            List<EscalationNew> escalationsToDelete = _dbContext.NewEscalations.Where(rec => rec.TimeSheetTableId == record.Id).ToList();
            List<SupportNew> supportsToDelete = _dbContext.NewSupportsSet.Where(rec => rec.TimeSheetTableId == record.Id).ToList();
            List<RiskNew> risksToDelete = _dbContext.NewRiskSet.Where(rec => rec.TimeSheetTableId == record.Id).ToList();
            _dbContext.NewBusinessBlockSet.RemoveRange(businessBlocksToDelete);
            _dbContext.NewEscalations.RemoveRange(escalationsToDelete);
            _dbContext.NewSupportsSet.RemoveRange(supportsToDelete);
            _dbContext.NewRiskSet.RemoveRange(risksToDelete);
        }

        /// <summary>
        /// Проверяет пересекается ли переданная запись с другими во времени
        /// </summary>
        /// <param name="record"></param>
        /// <returns>true если пересекается, false если нет</returns>
        public bool IsCollisionedWithOtherRecords(TimeSheetTable record)
        {
            bool state = false;
            foreach (TimeSheetTable historyRecord in _dbContext.TimeSheetTableSet.Where(i => i.AnalyticId == record.AnalyticId))
            {
                if (historyRecord.Id != record.Id && isInInterval(record.TimeStart, record.TimeEnd, historyRecord.TimeStart, historyRecord.TimeEnd))
                {
                    state = true;
                }
            }
            return state;
        }

        /// <summary>
        /// Алгоритм проверки вхождение одного промежутка времени в другой
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="start2"></param>
        /// <param name="end2"></param>
        /// <returns></returns>
        private bool isInInterval(DateTime start, DateTime end, DateTime start2, DateTime end2)
        {
            return (start >= start2 && start < end2) || //начальная дата в интервале
                (end > start2 && end <= end2) || //конечная дата в интервале
                (start <= start2 && end >= end2); //промежуток времени между датами включает интервал
        }

        /// <summary>
        /// Отчет по аналитикам
        /// </summary>
        /// <param name="analytics"></param>
        /// <param name="TimeStart"></param>
        /// <param name="TimeEnd"></param>
        /// <returns></returns>
        public DataTable GetAnalyticsReport(Analytic[] analytics, DateTime TimeStart, DateTime TimeEnd)
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
                ReportEntity = _dbContext.TimeSheetTableSet.Include("BusinessBlocks").
                    Include("Supports").Include("Risks").Include("Escalations").
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

        public TimeSheetTable GetLastRecordWithSameProcess(int process_id, string userName)
        {
            Process process = _dbContext.ProcessSet.FirstOrDefault(proc => proc.Id == process_id);
            Analytic user = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName.ToLower()));
            return _dbContext.TimeSheetTableSet.
                Include(i=>i.BusinessBlocks).ThenInclude(i=>i.BusinessBlock).
                Include(i=>i.Risks).ThenInclude(i=>i.Risk).
                Include(i=>i.Escalations).ThenInclude(i=>i.Escalation).
                Include(i=>i.Supports).ThenInclude(i=>i.Supports).Include(i=>i.ClientWays).Include(i=>i.Formats).Include(i=>i.Process).
                OrderByDescending(rec => rec.Id).
                FirstOrDefault(rec => rec.Process_id == process.Id && rec.AnalyticId == user.Id);
        }

        public IEnumerable<TimeSheetTable> GetTimeSheetRecordsForAnalytic(string userName)
        {
            return _dbContext.TimeSheetTableSet.Where(i => i.Analytic.UserName.ToLower().Equals(userName.ToLower())).ToList();
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public double GetTimeSpent(string userName, DateTime start, DateTime end)
        {
            return _dbContext.TimeSheetTableSet.
                Where(record => record.Analytic.UserName.ToLower().Equals(userName.ToLower()) && record.TimeStart >= start && record.TimeEnd <= end && record.Process_id != 62 && record.Process_id != 63).
                Select(record => (double?)record.TimeSpent).
                Sum() / 60 ?? 0;
        }

        public int GetDaysWorkedCount(string userName, DateTime start, DateTime end)
        {
            Analytic analytic = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName.ToLower()));
            List<DateTime> records = _dbContext.TimeSheetTableSet.
                Where(record => record.AnalyticId == analytic.Id && record.TimeStart >= start && record.TimeEnd <= end).
                GroupBy(record => record.TimeStart.Date).Select(i=>i.Key).ToList();
            return records.Count();
        }
        public IEnumerable<Analytic> GetTeam(Analytic analytic) => _dbContext.AnalyticSet.
            Where(a =>
            a.HeadFuncId == analytic.HeadFuncId && (a.OtdelId == analytic.Id || a.UpravlenieId == analytic.UpravlenieId || analytic.DirectionId == a.DirectionId) ||
            a.Id == analytic.HeadFuncId ||
            a.Id == analytic.HeadAdmId ||
            a.HeadFuncId == analytic.Id).
            ToList();

    }
}
