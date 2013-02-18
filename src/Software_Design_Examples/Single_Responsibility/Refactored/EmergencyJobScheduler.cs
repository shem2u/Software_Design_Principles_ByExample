using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class EmergencyJobScheduler
    {
        private readonly ISendMessages _messenger;

        public EmergencyJobScheduler(ISendMessages messenger)
        {
            _messenger = messenger;
        }

        public ScheduledJobResult Schedule(JobRequest request)
        {
            using (var ctx = new SharedDbContext())
            {
                var scheduledJob = ctx.ScheduledJob.Create();
                scheduledJob.ScheduledOn = request.RequestedByDate;
                ctx.ScheduledJob.Add(scheduledJob);
                ctx.SaveChanges();
            }
            _messenger.Send(new EmergencyJobScheduled{ScheduledOn = request.RequestedByDate});
            return new ScheduledJobResult{ScheduledToStart = request.RequestedByDate, Errors = new List<IJobSchedulingError>()};
        }
    }
}