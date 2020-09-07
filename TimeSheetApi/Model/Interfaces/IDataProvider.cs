using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TimeSheetApi.Model.Entities;

namespace TimeSheetApi.Model.Interfaces
{
    public interface IDataProvider
    {
        IEnumerable<string> GetSubjectHints(Process process);
        IEnumerable<Process> GetProcesses();
        IEnumerable<BusinessBlock> GetBusinessBlocks();
        IEnumerable<Supports> GetSupports();
        IEnumerable<ClientWays> GetClientWays();
        IEnumerable<Escalation> GetEscalation();
        IEnumerable<Formats> GetFormat();
        IEnumerable<Risk> GetRisks();
        IEnumerable<Analytic> GetMyAnalyticsData(Analytic currentUser);
        IEnumerable<string> GetProcessBlocks();
        IEnumerable<string> GetSubBlocksNames();
        IEnumerable<TimeSheetTable> LoadTimeSheetRecords(DateTime date, Analytic user);
        IEnumerable<TimeSheetTable> GetTimeSheetRecordsForAnalytic(Analytic currentUser);
        IEnumerable<Analytic> GetTeam(Analytic analytic);
        void AddActivity(TimeSheetTable activity);
        Analytic LoadAnalyticData();
        void UpdateProcess(TimeSheetTable oldProcess, TimeSheetTable newProcess);
        void DeleteRecord(TimeSheetTable record);
        bool IsCollisionedWithOtherRecords(TimeSheetTable record);
        TimeSheetTable GetLastRecordWithSameProcess(Process process, Analytic user);
        void RemoveSelection(TimeSheetTable record);
        void Commit();
        double GetTimeSpent(Analytic analytic, DateTime start, DateTime end);
        int GetDaysWorkedCount(Analytic currentUser, DateTime lastMonthFirstDay, DateTime lastMonthLastDay);
    }
}
