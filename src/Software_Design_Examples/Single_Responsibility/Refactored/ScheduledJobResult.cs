using System;
using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class ScheduledJobResult
    {
        public DateTime ScheduledToStart { get; set; }
        public IEnumerable<IJobSchedulingError> Errors { get; set; }
    }
}