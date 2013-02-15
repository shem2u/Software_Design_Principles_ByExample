using System.Data.Entity;

namespace Software_Design_Examples.Single_Responsibility
{
    public interface ISingleResponsibilityContext
    {
        DbSet<AvailableJob> Job { get; set; }
        DbSet<ScheduledJob> ScheduledJob { get; set; } 
    }
}