using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Documents;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("Process")]
    public partial class Process
    {
        public int id { get; set; }

        [Required]
        public string procName { get; set; }

        public string Comment { get; set; }

        public bool? CommentNeeded { get; set; }

        public int Block_id { get; set; }

        public int SubBlockId { get; set; }

        public int ProcessType_id { get; set; }
        
        public int Result_id { get; set; }
        public override string ToString()
        {
            return $"{Block_id}.{SubBlockId}.{id} - {procName}";
        }
    }
}
