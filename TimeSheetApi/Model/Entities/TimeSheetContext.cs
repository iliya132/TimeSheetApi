using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TimeSheetApi.Model.Entities;

namespace TimeSheetApi.Model
{
    public class TimeSheetContext :DbContext
    {
        const string CONNECTION_STRING = @"Data Source=ilyaHome;Initial Catalog=TimeSheet;Integrated Security=True";
        public TimeSheetContext() : base()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(CONNECTION_STRING);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Analytic>().HasOne(c => c.AdminHead).WithMany().HasForeignKey(c => c.HeadAdmId);
            modelBuilder.Entity<Analytic>().HasOne(c => c.FunctionHead).WithMany().HasForeignKey(c => c.HeadFuncId);
        }
        public DbSet<Analytic> AnalyticSet { get; set; }
        public DbSet<Block> BlockSet { get; set; }
        public DbSet<BusinessBlock> BusinessBlockSet { get; set; }
        public DbSet<BusinessBlockNew> NewBusinessBlockSet { get; set; }
        public DbSet<ClientWays> ClientWaysSet { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Directions> DirectionsSet { get; set; }
        public DbSet<Escalation> EscalationsSet { get; set; }
        public DbSet<EscalationNew> NewEscalations { get; set; }
        public DbSet<Formats> FormatsSet { get; set; }
        public DbSet<Otdel> OtdelSet { get; set; }
        public DbSet<Positions> PositionsSet { get; set; }
        public DbSet<Process> ProcessSet { get; set; }
        public DbSet<ProcessType> ProcessTypeSet { get; set; }
        public DbSet<Result> ResultSet { get; set; }
        public DbSet<Risk> RiskSet { get; set; }
        public DbSet<RiskNew> NewRiskSet { get; set; }
        public DbSet<Role> RoleSet { get; set; }
        public DbSet<SubBlock> SubBlockSet { get; set; }
        public DbSet<Supports> SupportsSet { get; set; }
        public DbSet<SupportNew> NewSupportsSet { get; set; }
        public DbSet<TimeSheetTable> TimeSheetTableSet { get; set; }
        public DbSet<Upravlenie> UpravlenieSet { get; set; }
    }
}
