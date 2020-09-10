

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TimeSheetApi.Model.Entities
{
    [Table("NewBusinessBlockSet")]
    public class BusinessBlockNew
    {
        public int Id { get; set; }
        public int BusinessBlockId { get; set; }
        public int TimeSheetTableId { get; set; }

        [ForeignKey(nameof(BusinessBlockId))]
        
        public virtual BusinessBlock BusinessBlock { get; set; }

        [ForeignKey(nameof(TimeSheetTableId))]
        [JsonIgnore]
        public virtual TimeSheetTable TimeSheetTable { get; set; }
    }
}
