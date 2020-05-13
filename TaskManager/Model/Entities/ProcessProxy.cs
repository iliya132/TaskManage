using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Model.Entities
{
    [Table("ProcessProxy")]
    public class ProcessProxy :Base.BaseEntity
    {
        public int ProcessId { get; set; }
        public int EmployeeTask_Id { get; set; }

        [ForeignKey("EmployeeTask_Id")]
        public virtual TaskEntity ParentTask { get; set; }
    }
}
