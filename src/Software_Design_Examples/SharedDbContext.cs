using System.Data.Entity;
using Software_Design_Examples.Single_Responsibility;

namespace Software_Design_Examples
{
    public class SharedDbContext : DbContext, ISingleResponsibilityContext
    {
        public DbSet<AvailableJob> Job { get; set; }
        public DbSet<ScheduledJob> ScheduledJob { get; set; }
    }
}