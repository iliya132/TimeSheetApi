using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TimeSheetApi.Model.Entities;

namespace TimeSheetApi.Model.Interfaces
{
    public interface IDataProvider
    {
        IEnumerable<string> GetSubjectHints(int Process_id, string userName);
        bool ForcedToQuit();
        IEnumerable<Process> GetProcesses();
        IEnumerable<BusinessBlock> GetBusinessBlocks();
        IEnumerable<Supports> GetSupports();
        IEnumerable<ClientWays> GetClientWays();
        IEnumerable<Escalation> GetEscalation();
        IEnumerable<Formats> GetFormat();
        IEnumerable<Risk> GetRisks();
        IEnumerable<Analytic> GetMyAnalyticsData(string userName);
        IEnumerable<string> GetProcessBlocks();
        IEnumerable<string> GetSubBlocksNames();
        void AddActivity(TimeSheetTable activity);
        Analytic LoadAnalyticData(string userName);
        void UpdateProcess(int oldProcessId, TimeSheetTable newRecord);
        void DeleteRecord(int record_id);
        IEnumerable<TimeSheetTable> LoadTimeSheetRecords(DateTime date, string userName);
        void GetReport(int ReportType, Analytic[] analytics, DateTime start, DateTime end);
        bool IsCollisionedWithOtherRecords(TimeSheetTable record);
        bool IsAnalyticHasAccess(string userName);
        TimeSheetTable GetLastRecordWithSameProcess(int process_id, string userName);
        void RemoveSelection(int record_id);
        IEnumerable<TimeSheetTable> GetTimeSheetRecordsForAnalytic(string userName);
        void Commit();
        double GetTimeSpent(string userName, DateTime start, DateTime end);
        int GetDaysWorkedCount(string userName, DateTime lastMonthFirstDay, DateTime lastMonthLastDay);
        IEnumerable<Analytic> GetTeam(Analytic analytic);
    }
}
