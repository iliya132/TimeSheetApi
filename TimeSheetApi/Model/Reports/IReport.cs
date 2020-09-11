using System;
using System.IO;

namespace TimeSheetApp.Model.Reports
{
    public interface IReport
    {
        FileInfo Generate(DateTime start, DateTime end);
    }
}