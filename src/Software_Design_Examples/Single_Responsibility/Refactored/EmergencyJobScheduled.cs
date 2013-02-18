using System;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class EmergencyJobScheduled : IMessage
    {
        public DateTime ScheduledOn { get; set; }
    }
}