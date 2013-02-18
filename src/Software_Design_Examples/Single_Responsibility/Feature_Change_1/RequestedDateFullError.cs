using System;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_1
{
    public class RequestedDateFullError : IJobRequestError
    {
        public DateTime NextAvailableDate { get; set; }
    }
}