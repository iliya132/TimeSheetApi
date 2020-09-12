using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using TimeSheetApi.Model;
using TimeSheetApi.Model.Entities;

namespace TimeSheetApp.Model.Reports
{
    public abstract class IReport
    {
        internal TimeSheetContext _dbContext { get; set; }
        internal List<Analytic> analytics { get; set; }
        public IReport(TimeSheetContext _dbContext, IEnumerable<Analytic> analytics)
        {
            this._dbContext = _dbContext;
            this.analytics = new List<Analytic>(analytics);
        }
        public abstract FileInfo Generate(DateTime start, DateTime end);
    }
}