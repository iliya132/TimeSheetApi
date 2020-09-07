using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities
{
    [Table("UpravlenieTable")]
    public class Upravlenie
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string shortName { get; set; }
    }
}
