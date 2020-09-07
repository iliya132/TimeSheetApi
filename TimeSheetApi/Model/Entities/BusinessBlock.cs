using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities
{
    [Table("BusinessBlock")]
    public class BusinessBlock
    {
        [Key]
        public int Id { get; set; }
        public string BusinessBlockName { get; set; }
    }
}
