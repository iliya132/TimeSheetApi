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
        public IEnumerable<string> GetSubjectHints(Process process) => _dbProvider.GetSubjectHints(process);

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
        public IEnumerable<Analytic> GetMyAnalyticsData(Analytic currentUser) => _dbProvider.GetMyAnalyticsData(currentUser);

        [HttpGet]
        [Route(nameof(GetProcessBlocks))]
        public IEnumerable<string> GetProcessBlocks() => _dbProvider.GetProcessBlocks();

        [HttpGet]
        [Route(nameof(GetSubBlocksNames))]
        public IEnumerable<string> GetSubBlocksNames() => _dbProvider.GetSubBlocksNames();

        [HttpGet]
        [Route(nameof(LoadTimeSheetRecords))]
        public IEnumerable<TimeSheetTable> LoadTimeSheetRecords((DateTime date, Analytic user) input) => _dbProvider.LoadTimeSheetRecords(input.date, input.user);

        [HttpGet]
        [Route(nameof(GetTimeSheetRecordsForAnalytic))]
        public IEnumerable<TimeSheetTable> GetTimeSheetRecordsForAnalytic(Analytic currentUser) => _dbProvider.GetTimeSheetRecordsForAnalytic(currentUser);

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
        public void DeleteRecord(TimeSheetTable record) => _dbProvider.DeleteRecord(record);

        [HttpGet]
        [Route(nameof(IsCollisionedWithOtherRecords))]
        public bool IsCollisionedWithOtherRecords(TimeSheetTable record) => _dbProvider.IsCollisionedWithOtherRecords(record);

        [HttpGet]
        [Route(nameof(GetLastRecordWithSameProcess))]
        public TimeSheetTable GetLastRecordWithSameProcess((Process process, Analytic user) input) => _dbProvider.GetLastRecordWithSameProcess(input.process, input.user);

        [HttpDelete]
        [Route(nameof(RemoveSelection))]
        public void RemoveSelection(TimeSheetTable record) => _dbProvider.RemoveSelection(record);

        [HttpPost]
        [Route(nameof(Commit))]
        public void Commit() => _dbProvider.Commit();

        [HttpGet]
        [Route(nameof(GetTimeSpent))]
        public double GetTimeSpent((Analytic analytic, DateTime start, DateTime end) input) => _dbProvider.GetTimeSpent(input.analytic, input.start, input.end);

        [HttpGet]
        [Route(nameof(GetDaysWorkedCount))]
        public int GetDaysWorkedCount((Analytic currentUser, DateTime lastMonthFirstDay, DateTime lastMonthLastDay) input) => _dbProvider.GetDaysWorkedCount(input.currentUser, input.lastMonthFirstDay, input.lastMonthLastDay);
    }
}
