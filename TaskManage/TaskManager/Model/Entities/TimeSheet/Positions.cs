using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("Positions")]
    public class Positions
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
