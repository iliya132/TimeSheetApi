using System.Collections.Generic;

using TimeSheetApi.Model.Entities;

namespace TimeSheetApp.Model.Report.Report_Allocation
{
    public class MVZ
    {
#if DevAtHome
        public enum AllocationRule :int
        {
            RETAIL_BUSINESS =1,
            A_CLUB = 2,
            MASS_BUSINESS = 3,
            MEDIUM_BUSINESS = 4,
            BIG_BUSINESS = 5,
            TREASURY = 7,
            INVESTMENT = 8,
            Broker = 9
        }
#else
        public enum AllocationRule : int
        {
            RETAIL_BUSINESS = 1,
            A_CLUB = 2,
            MASS_BUSINESS = 3,
            MEDIUM_BUSINESS = 4,
            BIG_BUSINESS = 5,
            TREASURY = 6,
            INVESTMENT = 8,
            Broker = 9
        }
#endif
        public string Name { get; set; }
        public string UnitName { get; set; }
        public List<Analytic> Analytics { get; set; }
        public Dictionary<AllocationRule, int> AllocationRules { get; set; }
    }
}
