using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TimeSheetApi.Model.Entities
{
    [Table("NewSupportsSet")]
    public class SupportNew
    {
        public int Id { get; set; }
        public int SupportId { get; set; }
        public int TimeSheetTableId { get; set; }

        [ForeignKey(nameof(SupportId))]
        public Supports Supports { get; set; }

        [ForeignKey(nameof(TimeSheetTableId))]
        [JsonIgnore]
        public TimeSheetTable TimeSheetTable { get; set; }
    }
}
