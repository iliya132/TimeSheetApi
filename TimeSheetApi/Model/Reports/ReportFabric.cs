using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TimeSheetApi.Model.Entities;

using TimeSheetApp.Model.Reports;

namespace TimeSheetApi.Model.Reports
{
    public static class ReportFabric
    {
        public static IReport GetReport(int ReportId, Analytic[] analytics, TimeSheetContext timeSheetContext)
        {
            switch (ReportId)
            {
                case (1):return new AnalyticActivityReport_01(timeSheetContext, analytics);
                case (2):return new Report_02(timeSheetContext, analytics);
                case (3):return new Report_03(timeSheetContext, analytics);
                case (4):return new Report_Allocations(timeSheetContext, analytics);
                default: return null;
            }
        }
    }
}
