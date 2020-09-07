﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities

{
    [Table("NewEscalations")]
    public class EscalationNew
    {
        public int Id { get; set; }
        public int EscalationId { get; set; }
        public int TimeSheetTableId { get; set; }

        [ForeignKey(nameof(EscalationId))]
        public virtual Escalation Escalation { get; set; }
        [ForeignKey(nameof(TimeSheetTableId))]
        public virtual TimeSheetTable TimeSheetTable { get; set; }
    }
}
