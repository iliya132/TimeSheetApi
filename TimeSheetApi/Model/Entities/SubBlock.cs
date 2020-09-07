using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities
{
    [Table("SubBlock")]
    public class SubBlock
    {
        [Key]
        public int Id { get; set; }
        [Column("subblockName")]
        public string SubblockName { get; set; }
    }
}
