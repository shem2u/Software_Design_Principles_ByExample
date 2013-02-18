using System;
using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.An_Initial_Design
{
    /// <summary>
    /// This class is responsible for scheduling engineers when given a 
    /// valid request for work.
    /// </summary>
    public class JobProcessor
    {
        public JobRequestResult Process(JobRequest request)
        {
            if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestError> {new JobInThePastError()}};
            using (var ctx = new SharedDbContext())
            {
                var matchingTasks = ctx.Job.Find(request.RequestedTask);
                if(matchingTasks == null)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestError> {new UnknownJobIdError()}};
                var scheduledJob = ctx.ScheduledJob.Create();
                scheduledJob.ScheduledOn = request.RequestedByDate;
                ctx.ScheduledJob.Add(scheduledJob);
                ctx.SaveChanges();
            }
            return new JobRequestResult {Accepted = true, ScheduledToBeginOn = request.RequestedByDate};
        }
    }
}