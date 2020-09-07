using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeSheetApi.Model.Entities
{
    [Table("Escalations")]
    public class Escalation
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
