using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("UpravlenieTable")]
    public class Upravlenie
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
