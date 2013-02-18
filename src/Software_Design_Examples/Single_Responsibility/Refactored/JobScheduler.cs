using System;
using System.Collections.Generic;
using System.Linq;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class JobScheduler
    {
        public ScheduledJobResult Schedule(JobRequest request)
        {
            using (var context = new SharedDbContext())
            {
                var scheduledJob = context.ScheduledJob.Create();
                var maximumNumberOfJobsAlreadyScheduled = (from job in context.ScheduledJob
                                                           where job.ScheduledOn.Equals(request.RequestedByDate)
                                                           select job).Count() > 3;
                if(maximumNumberOfJobsAlreadyScheduled)
                {
                    var date = GetNextAvailableDate(context, request.RequestedByDate);
                    scheduledJob.ScheduledOn = date;
                    context.ScheduledJob.Add(scheduledJob);
                    context.SaveChanges();
                    return new ScheduledJobResult
                        {
                            ScheduledToStart = date, Errors = new List<IJobSchedulingError>{new RequestedDateFullError()}
                        };
                }
                scheduledJob.ScheduledOn = request.RequestedByDate;
                context.ScheduledJob.Add(scheduledJob);
                context.SaveChanges();
            }
            return new ScheduledJobResult{ScheduledToStart = request.RequestedByDate};
        }

        private DateTime GetNextAvailableDate(SharedDbContext context, DateTime startfrom)
        {
            var date = startfrom;
            do
            {
                date = date.AddDays(1);
            } while ((from job in context.ScheduledJob where job.ScheduledOn.Equals(date) select job).Count() > 3);
            return date;
        }
    }
}