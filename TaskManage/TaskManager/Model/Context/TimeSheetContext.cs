namespace TaskManager.Model.Context
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using TaskManager.Model.Entities.TimeSheet;

    public class TimeSheetContext : DbContext
    {
        // Your context has been configured to use a 'TimeSheetContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'TaskManager.Model.Context.TimeSheetContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'TimeSheetContext' 
        // connection string in the application configuration file.
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