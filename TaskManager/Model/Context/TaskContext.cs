namespace TaskManager.Model.Context
{
    using System.Data.Entity;
    using TaskManager.Model.Entities;

    public class TaskContext : DbContext
    {
        public TaskContext()
            : base("name=TaskContext")
        {
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ProcessProxy> Processes { get; set; }
    }
}