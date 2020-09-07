using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TimeSheetApi.Model.Entities;
using TimeSheetApi.Model.Interfaces;

namespace TimeSheetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSheetController : ControllerBase
    {
        IDataProvider _dbProvider;
        public TimeSheetController(IDataProvider dataProvider)
        {
            _dbProvider = dataProvider;
        }

        [Route("GetHints")]
        IEnumerable<string> GetSubjectHints(Process process) => _dbProvider.GetSubjectHints(process);

    }
}
