using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TimeSheetApi.Model.Entities;
using TimeSheetApi.Model.Interfaces;

namespace TimeSheetApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TimeSheetController : ControllerBase
    {
        IDataProvider _dbProvider;

        public TimeSheetController(IDataProvider dataProvider)
        {
            _dbProvider = dataProvider;
        }

        [HttpGet]
        [Route(nameof(GetSubjectHints))]
        public IEnumerable<string> GetSubjectHints(int process_id, string userName) => _dbProvider.GetSubjectHints(process_id, userName);

        [HttpGet]
        [Route(nameof(GetProcesses))]
        public IEnumerable<Process> GetProcesses() => _dbProvider.GetProcesses();

        [HttpGet]
        [Route(nameof(GetBusinessBlocks))]
        public IEnumerable<BusinessBlock> GetBusinessBlocks() => _dbProvider.GetBusinessBlocks();

        [HttpGet]
        [Route(nameof(GetSupports))]
        public IEnumerable<Supports> GetSupports() => _dbProvider.GetSupports();

        [HttpGet]
        [Route(nameof(GetClientWays))]
        public IEnumerable<ClientWays> GetClientWays() => _dbProvider.GetClientWays();

        [HttpGet]
        [Route(nameof(GetEscalation))]
        public IEnumerable<Escalation> GetEscalation() => _dbProvider.GetEscalation();

        [HttpGet]
        [Route(nameof(GetFormat))]
        public IEnumerable<Formats> GetFormat() => _dbProvider.GetFormat();

        [HttpGet]
        [Route(nameof(GetRisks))]
        public IEnumerable<Risk> GetRisks() => _dbProvider.GetRisks();

        [HttpGet]
        [Route(nameof(GetMyAnalyticsData))]
        public IEnumerable<Analytic> GetMyAnalyticsData(string userName) => _dbProvider.GetMyAnalyticsData(userName);

        [HttpGet]
        [Route(nameof(GetProcessBlocks))]
        public IEnumerable<string> GetProcessBlocks() => _dbProvider.GetProcessBlocks();

        [HttpGet]
        [Route(nameof(GetSubBlocksNames))]
        public IEnumerable<string> GetSubBlocksNames() => _dbProvider.GetSubBlocksNames();

        [HttpGet]
        [Route(nameof(LoadTimeSheetRecords))]
        public IEnumerable<TimeSheetTable> LoadTimeSheetRecords((DateTime date, string userName) input) => _dbProvider.LoadTimeSheetRecords(input.date, input.userName);

        [HttpGet]
        [Route(nameof(GetTimeSheetRecordsForAnalytic))]
        public IEnumerable<TimeSheetTable> GetTimeSheetRecordsForAnalytic(string userName) => _dbProvider.GetTimeSheetRecordsForAnalytic(userName);

        [HttpGet]
        [Route(nameof(GetTeam))]
        public IEnumerable<Analytic> GetTeam(Analytic analytic) => _dbProvider.GetTeam(analytic);

        [HttpPost]
        [Route(nameof(AddActivity))]
        public void AddActivity(TimeSheetTable activity) => _dbProvider.AddActivity(activity);

        [HttpGet]
        [Route(nameof(LoadAnalyticData))]
        public Analytic LoadAnalyticData(string UserName) => _dbProvider.LoadAnalyticData(UserName);

        [HttpPost]
        [Route(nameof(UpdateProcess))]
        public void UpdateProcess((TimeSheetTable oldProcess, TimeSheetTable newProcess) input) => _dbProvider.UpdateProcess(input.oldProcess, input.newProcess);

        [HttpDelete]
        [Route(nameof(DeleteRecord))]
        public void DeleteRecord(int record_id) => _dbProvider.DeleteRecord(record_id);

        [HttpGet]
        [Route(nameof(IsCollisionedWithOtherRecords))]
        public bool IsCollisionedWithOtherRecords(TimeSheetTable record) => _dbProvider.IsCollisionedWithOtherRecords(record);

        [HttpGet]
        [Route(nameof(GetLastRecordWithSameProcess))]
        public TimeSheetTable GetLastRecordWithSameProcess(int process_id, string userName) => _dbProvider.GetLastRecordWithSameProcess(process_id, userName);

        [HttpDelete]
        [Route(nameof(RemoveSelection))]
        public void RemoveSelection(int record_id) => _dbProvider.RemoveSelection(record_id);

        [HttpPost]
        [Route(nameof(Commit))]
        public void Commit() => _dbProvider.Commit();

        [HttpGet]
        [Route(nameof(GetTimeSpent))]
        public double GetTimeSpent(string userName, DateTime start, DateTime end) => _dbProvider.GetTimeSpent(userName, start, end);

        [HttpGet]
        [Route(nameof(GetDaysWorkedCount))]
        public int GetDaysWorkedCount(string userName, DateTime lastMonthFirstDay, DateTime lastMonthLastDay) => _dbProvider.GetDaysWorkedCount(userName, lastMonthFirstDay, lastMonthLastDay);
    }
}
