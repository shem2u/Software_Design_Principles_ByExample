using System;
using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class JobRequestResult
    {
        public bool Accepted { get; set; }
        public IEnumerable<IJobRequestError> Errors { get; set; }
        public DateTime ScheduledToStart { get; set; }
    }
}