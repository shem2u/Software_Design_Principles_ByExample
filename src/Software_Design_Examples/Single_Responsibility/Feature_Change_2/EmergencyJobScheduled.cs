using System;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_2
{
    public class EmergencyJobScheduled : IMessage
    {
        public DateTime ScheduledDate { get; set; }
    }
}