using System;
using System.Collections.Generic;
using System.Linq;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_1
{
    /// <summary>
    /// This class is responsible for scheduling engineers when given a 
    /// valid request for work.
    /// </summary>
    public class JobProcessor
    {
        public JobRequestResult Process(JobRequest request) {
#region Unaffected By Change
            if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestErrors> {new JobInThePastError()}};
            using (var ctx = new SharedDbContext())
            {
                var matchingTasks = ctx.Job.Find(request.RequestedTask);
                if(matchingTasks == null)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestErrors> {new UnknownJobIdError()}};
#endregion Unaffected By Change
#region New Code to Support limiting number of jobs per day to 4
                var maximumNumberOfJobsAlreadyScheduled = (from job in ctx.ScheduledJob
                                                           where job.ScheduledOn.Equals(request.RequestedByDate)
                                                           select job).Count() > 3;
                if (maximumNumberOfJobsAlreadyScheduled)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestErrors> {new RequestedDateFullError()}};
#endregion New Code to Support limiting number of jobs per day to 4
#region Unaffected By Change
                var scheduledJob = ctx.ScheduledJob.Create();
                scheduledJob.ScheduledOn = request.RequestedByDate;
                ctx.ScheduledJob.Add(scheduledJob);
                ctx.SaveChanges();
            }
            return new JobRequestResult {Accepted = true, ScheduledToBeginOn = request.RequestedByDate};
#endregion Unaffected By Change
        }
    }
}