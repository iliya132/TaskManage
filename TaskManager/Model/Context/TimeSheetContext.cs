namespace TaskManager.Model.Context
{
    using System.Data.Entity;
    using TaskManager.Model.Entities.TimeSheet;

    public class TimeSheetContext : DbContext
    {
        public TimeSheetContext()
            : base("name=TimeSheetContext")
        {
        }

        public DbSet<Departments> Departments { get; set; }
        public DbSet<Directions> DirectionsSet { get; set; }
        public DbSet<Otdel> OtdelSet { get; set; }
        public DbSet<Positions> PositionsSet { get; set; }
        public DbSet<Role> RoleSet { get; set; }
        public DbSet<Upravlenie> UpravlenieSet { get; set; }
        public virtual DbSet<Process> Process { get; set; }
        public virtual DbSet<Analytic> Analytics { get; set; }

    }
}