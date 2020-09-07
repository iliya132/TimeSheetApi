﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities
{
    [Table("NewRiskSet")]
    public class RiskNew
    {
        public int Id { get; set; }
        public int RiskId { get; set; }
        public int TimeSheetTableId { get; set; }

        [ForeignKey(nameof(RiskId))]
        public virtual Risk Risk { get; set; }
        [ForeignKey(nameof(TimeSheetTableId))]
        public virtual TimeSheetTable TimeSheetTable { get; set; }
    }
}
