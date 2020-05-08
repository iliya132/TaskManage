using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities.TimeSheet
{
    [Table("DirectionsSet")]
    public class Directions
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
