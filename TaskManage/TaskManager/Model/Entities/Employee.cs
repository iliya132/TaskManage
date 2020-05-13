using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities
{
    public class Employee : Base.NamedEntity
    {
        [Column("FIO")]
        new public string Name { get; set; }
        public int AnalyticId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
