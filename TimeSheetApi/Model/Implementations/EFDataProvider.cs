using System;
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
using System.IO;
using TimeSheetApp.Model;
using TimeSheetApp.Model.Reports;
using TimeSheetApi.Model.Reports;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TimeSheetApi.Model.Implementations
{
    class EFDataProvider : IDataProvider
    {
        TimeSheetContext _dbContext = new TimeSheetContext();
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
            if (Process_id > 0 && Process_id < 64)
            {
                return _dbContext.TimeSheetTableSet.
                    Where(i => i.Analytic.UserName.ToLower().Equals(userName.ToLower()) &&
                        i.Subject.Length > 0 &&
                        i.Process_id == Process_id).OrderBy(i => i.TimeStart).Select(i => i.Subject).ToList();
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
        public TimeSheetTable AddActivity(TimeSheetTable newRecord)
        {
            newRecord.TimeSpent = (int)(newRecord.TimeEnd - newRecord.TimeStart).TotalMinutes;
            newRecord.AnalyticId = newRecord.Analytic != null ? newRecord.Analytic.Id : newRecord.AnalyticId;
            newRecord.Analytic = null;
            _dbContext.TimeSheetTableSet.Add(newRecord);
            _dbContext.SaveChanges();
            
            return _dbContext.TimeSheetTableSet.
                Include(i=>i.Analytic).
                Include(i=>i.BusinessBlocks).
                    ThenInclude(i=>i.BusinessBlock).
                Include(i=>i.ClientWays).
                Include(i=>i.Escalations).
                    ThenInclude(i=>i.Escalation).
                Include(i=>i.Formats).
                Include(i=>i.Process).
                Include(i=>i.Risks).
                    ThenInclude(i=>i.Risk).
                Include(i=>i.Supports).
                    ThenInclude(i=>i.Supports).
                FirstOrDefault(i => i.Id == newRecord.Id);
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
        public FileInfo GetReport(int ReportType, string analytics, DateTime start, DateTime end)
        {
            List<Analytic> analyticsList = new List<Analytic>();
            foreach(string str in analytics.Split('*'))
            {
                analyticsList.
                    Add(_dbContext.AnalyticSet.
                    Include(i=>i.Departments).
                    Include(i=>i.Directions).
                    Include(i=>i.Upravlenie).
                    Include(i=>i.Otdel).
                    FirstOrDefault(a => a.Id == Convert.ToInt32(str)));
            }

            IReport report = ReportFabric.GetReport(ReportType, analyticsList.ToArray(), _dbContext);
            return report.Generate(start, end);
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
        public bool IsCollisionedWithOtherRecords(DateTime start, DateTime end, int analyticId, int recId)
        {
            foreach (TimeSheetTable historyRecord in _dbContext.TimeSheetTableSet.Where(i => i.AnalyticId == analyticId).ToList())
            {
                if (historyRecord.Id != recId && isInInterval(start, end, historyRecord.TimeStart, historyRecord.TimeEnd))
                {
                    return true;
                }
            }
            return false;
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

        public IEnumerable<Process> GetProcessesSortedByRelevance(string userName, string filter)
        {
            IOrderedEnumerable<Process> result;
            List<Process> processes = _dbContext.ProcessSet.ToList();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                processes = processes.Where(process =>
                           process.ProcName.ToLower().IndexOf(filter.ToLower()) > -1
                           || process.CodeFull.IndexOf(filter) >-1
                        || process.Comment?.ToLower().IndexOf(filter.ToLower()) > -1).ToList();
                
            }
            result = processes.OrderByDescending(process => _dbContext.TimeSheetTableSet.
                        Where(rec => rec.Analytic.UserName.ToLower().Equals(userName.ToLower())
                        && rec.Process_id == process.Id).
                        Count()).
                ThenBy(proc => proc.Id);
            return result;
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
            int analyticId = _dbContext.AnalyticSet.FirstOrDefault(i => i.UserName.ToLower().Equals(userName.ToLower())).Id;
            return _dbContext.TimeSheetTableSet.
                Where(record => record.AnalyticId == analyticId && record.TimeStart >= start && record.TimeEnd <= end).Select(i=>i.TimeStart.Date).
                Distinct().Count();
        }

        public IEnumerable<Analytic> GetTeam(Analytic analytic) => _dbContext.AnalyticSet.
            Where(a =>
            a.HeadFuncId == analytic.HeadFuncId && (a.OtdelId == analytic.Id || a.UpravlenieId == analytic.UpravlenieId || analytic.DirectionId == a.DirectionId) ||
            a.Id == analytic.HeadFuncId ||
            a.Id == analytic.HeadAdmId ||
            a.HeadFuncId == analytic.Id).
            ToList();

        public IEnumerable<string> GetReportsAvailable()
        {
            var result = _dbContext.Reports.Select(i => i.Name);
            return result;
        }
    }
}
