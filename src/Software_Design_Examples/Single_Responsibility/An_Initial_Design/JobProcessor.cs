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
            if (request.RequestedTask != 12345)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestErrors> {new UnknownJobIdError()}};
            if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                return new JobRequestResult
                    {Accepted = false, Errors = new List<IJobRequestErrors> {new JobInThePastError()}};
            return new JobRequestResult {Accepted = true, ScheduledToBeginOn = request.RequestedByDate};
        }
    }
}