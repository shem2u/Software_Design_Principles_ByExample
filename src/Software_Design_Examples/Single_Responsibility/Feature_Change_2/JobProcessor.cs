using System;
using System.Collections.Generic;
using System.Linq;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;
using Software_Design_Examples.Single_Responsibility.Feature_Change_1;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_2
{
    /// <summary>
    /// This class is responsible for scheduling engineers when given a 
    /// valid request for work.
    /// 
    /// See Software_Design_Examples_Tests.Single_Responsibility.Second_Feature_Change_Specification
    /// for the requirements driving the change below.
    /// </summary>
    public class JobProcessor
    {
        public virtual JobRequestResult Process(JobRequest request) {
            #region Unaffected logic
            if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestError> {new JobInThePastError()}};
            #endregion Unaffected logic
            #region Altered Logic to support 3 days in the future limit for non-emergency requests
            if (request.RequestedByDate.CompareTo(DateTime.Now.AddDays(2)) <= 0)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestError> {new RequestedDateTooSoonError()}};
            #endregion Altered Logic to support 2 days in the future limit for non-emergency requests
            #region Unaffected logic
            using (var ctx = new SharedDbContext())
            {
                var matchingTasks = ctx.Job.Find(request.RequestedTask);
                if (matchingTasks == null)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestError> {new UnknownJobIdError()}};
                var maximumNumberOfJobsAlreadyScheduled = (from job in ctx.ScheduledJob
                                                           where job.ScheduledOn.Equals(request.RequestedByDate)
                                                           select job).Count() > 3;
                if (maximumNumberOfJobsAlreadyScheduled)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestError> {new RequestedDateFullError()}};
                var scheduledJob = ctx.ScheduledJob.Create();
                scheduledJob.ScheduledOn = request.RequestedByDate;
                ctx.ScheduledJob.Add(scheduledJob);
                ctx.SaveChanges();
            }
            return new JobRequestResult {Accepted = true, ScheduledToBeginOn = request.RequestedByDate};
            #endregion Unaffected logic
        }
    }
}