using System;
using System.Collections.Generic;
using System.Linq;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;
using Software_Design_Examples.Single_Responsibility.Feature_Change_1;

namespace Software_Design_Examples.Single_Responsibility.Feature_Change_2
{
    /// <summary>
    /// This class handles emergency job requests and passes 
    /// regular job requests onto the base class.
    /// It could be considered a Decorator (e.g. following the
    /// decorator pattern) although it should not be considered
    /// a cannonical example.
    /// 
    /// It is also an example of a common (mis) use of inheritence - 
    /// to save keystrokes without refactoring the solution design.
    /// 
    /// If you have NCrunch or another test coverage tool enabled
    /// you should notice which behaviors (lines) of this class are
    /// not covered and compare them to the JobProcessor coverage.
    /// Interesting huh?  What does that tell you about these classes?
    /// </summary>
    public class UberJobProcessor : JobProcessor
    {
        private readonly ISendMessages _messenger;

        public UberJobProcessor(ISendMessages messenger)
        {
            _messenger = messenger;
        }

        public override JobRequestResult Process(JobRequest request) 
        {
            if(request.IsEmergency)
            {
                if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                    return new JobRequestResult
                        {Accepted = false, Errors = new List<IJobRequestErrors> {new JobInThePastError()}};
                using (var ctx = new SharedDbContext())
                {
                    var matchingTasks = ctx.Job.Find(request.RequestedTask);
                    if (matchingTasks == null)
                        return new JobRequestResult
                            {Accepted = false, Errors = new List<IJobRequestErrors> {new UnknownJobIdError()}};
                    var scheduledJob = ctx.ScheduledJob.Create();
                    scheduledJob.ScheduledOn = request.RequestedByDate;
                    ctx.ScheduledJob.Add(scheduledJob);
                    ctx.SaveChanges();
                }
                _messenger.Send(new EmergencyJobScheduled{ ScheduledDate = request.RequestedByDate});
                return new JobRequestResult {Accepted = true, ScheduledToBeginOn = request.RequestedByDate};
            }
            return base.Process(request);
        }
    }
}