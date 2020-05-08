using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("OtdelTable")]
    public class Otdel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
