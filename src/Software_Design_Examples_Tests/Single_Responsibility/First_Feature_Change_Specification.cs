using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;
using Software_Design_Examples.Single_Responsibility.Feature_Change_1;

namespace Software_Design_Examples_Tests.Single_Responsibility
{
    /// <summary>
    /// This specification details changes needed to support a new 
    /// behavior for the job scheduling system specified in An_Initial_Design_Specification.
    /// Although the initial specification (tests) strongly hinted at the existence of more
    /// than one responsibilty in the class, this change and the area of the code affected
    /// (or not) begins to highlight the diferentiation of responsibility.
    /// 
    /// *** Context ***
    /// The system was deployed and functioned as designed but an issue was discovered - 
    /// the Engineering Team could only accomodate an average of 4 jobs a day.  The system
    /// had no contraints on the number of jobs that could be scheduled for a given day.
    /// Most of the time - and for the first several months of the system's life - this did
    /// not present an issue, but eventually several days were overbooked and the team had
    /// to work extra hours to accomodate the requests.  It was requested that the system be 
    /// altered to limit the number of jobs per day to 4.
    /// 
    /// Success Scenario:
    /// Given a Building Administrator with rights to request jobs
    /// And no more than 3 jobs currently scheduled for the requested day
    /// When the Administrator submits the job request
    /// Then the System should accept the request
    /// And Schedule the work to be started on the requested day
    /// 
    /// Too Many Requests Scenario:
    /// Given a Building Administrator with rights to request jobs
    /// And more than 4 jobs currently scheduled for the requested day
    /// When the Administrator submits the job request
    /// Then the System should reject the request
    /// 
    /// See wiki for details ()
    /// </summary>
    [TestClass]
    public class First_Feature_Change_Specification : IntegratedDbTest
    {
        private static DateTime _dateWithTooManyScheduledJobs = DateTime.Now.AddDays(4);
        [ClassInitialize]
        public static void OnTestInit(TestContext unused)
        {
            WithDbContext(context =>
                {
                    var knownJob = context.Job.Create();
                    knownJob.Description = "Testing Initial Design";
                    context.Job.Add(knownJob);
                    var scheduledJob1 = context.ScheduledJob.Create();
                    var scheduledJob2 = context.ScheduledJob.Create();
                    var scheduledJob3 = context.ScheduledJob.Create();
                    var scheduledJob4 = context.ScheduledJob.Create();
                    scheduledJob1.ScheduledOn = _dateWithTooManyScheduledJobs;
                    scheduledJob2.ScheduledOn = _dateWithTooManyScheduledJobs;
                    scheduledJob3.ScheduledOn = _dateWithTooManyScheduledJobs;
                    scheduledJob4.ScheduledOn = _dateWithTooManyScheduledJobs;
                    context.ScheduledJob.Add(scheduledJob1);
                    context.ScheduledJob.Add(scheduledJob2);
                    context.ScheduledJob.Add(scheduledJob3);
                    context.ScheduledJob.Add(scheduledJob4);
                    context.SaveChanges();
                });
        }

        private int GetKnownTaskId()
        {
            var taskId = 0;
            WithDbContext(ctx => taskId = ctx.Job.First().Id);
            return taskId;
        }

        [TestMethod]
        public void Should_Schedule_Valid_Book_Job_Request_If_Scheduled_Work_Acceptable_for_Requested_Date()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = GetKnownTaskId()};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.ScheduledToBeginOn.Date.ShouldEqual(request.RequestedByDate.Date);
        }

        [TestMethod]
        public void Should_Reject_Valid_Book_Job_Request_If_Requested_Date_Full()
        {
            var request = new JobRequest{RequestedByDate = _dateWithTooManyScheduledJobs, RequestedTask = GetKnownTaskId()};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(RequestedDateFullError)).ShouldBeTrue();
        }

        #region Existing Behaviors

        [TestMethod]
        public void Should_Accept_Valid_Book_Job_Command()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = GetKnownTaskId()};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.Accepted.ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_The_Past_With_Input_Errors()
        {
            var request = new JobRequest
                {RequestedByDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)), RequestedTask = GetKnownTaskId()};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(JobInThePastError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_Unknown_Task_Id_With_UnknownJobError()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(2), RequestedTask = 1231354957};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(UnknownJobIdError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Schedule_Job_Based_On_Results()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = GetKnownTaskId()};
            var sut = new Software_Design_Examples.Single_Responsibility.Feature_Change_1.JobProcessor();
            var result = sut.Process(request);
            result.ScheduledToBeginOn.CompareTo(request.RequestedByDate).ShouldBeInRange(-14, 0);
        }

        #endregion Existing Behaviors
    }
}