using System;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_2
{
    public class JobRequest
    {
        public int RequestedTask { get; set; }
        public DateTime RequestedByDate { get; set; }

        public bool IsEmergency { get; set; }
    }
}