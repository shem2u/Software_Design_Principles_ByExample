using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;
using Software_Design_Examples.Single_Responsibility.Feature_Change_1;
using Software_Design_Examples.Single_Responsibility.Feature_Change_2;
using JobRequest = Software_Design_Examples.Single_Responsibility.Feature_Change_2.JobRequest;

namespace Software_Design_Examples_Tests.Single_Responsibility
{
    /// <summary>
    /// This specification details changes needed to support a new 
    /// behavior for the job scheduling system specified in Feature_Change_1_Specification.
    /// The solution provided in Feature_Change_2.JobProcessor and Feature_Change_2.UberJobProcessor
    /// bring the existence of too many responsibilities in sharp focus.  The decision to use
    /// inheritence in the solution should provide further clarity of the responsibilities.
    /// 
    /// *** Context ***
    /// 
    /// Non-Emergency Success Scenario:
    /// Given a Building Administrator with rights to request jobs
    /// And no more than 3 jobs currently scheduled for the requested day
    /// And the requested day at least 3 days in the future
    /// When the Administrator submits the job request
    /// Then the System should accept the request
    /// And Schedule the work to be started on the requested day
    /// 
    /// Non-Emergency Date is too soon Scenario:
    /// Given a Building Administrator with rights to request jobs
    /// And no more than 3 jobs currently scheduled for the requested day
    /// And the requested day less than 3 days in the future
    /// When the Administrator submits the job request
    /// Then the System should reject the request
    /// 
    /// Emergency Request Scenario:
    /// Given a Building Administrator with rights to request jobs
    /// And an emergency request
    /// When the Administrator submits the job request
    /// Then the System should accept the request
    /// And schedule the job on the requested date
    /// And send an text message to the Engineer staff including how many jobs are scheduled
    /// 
    /// See wiki for details (https://github.com/shem2u/Software_Design_Principles_ByExample/wiki/Single-Responsibility)
    /// </summary>
    [TestClass]
    public class Feature_Change_2_Specification : IntegratedDbTest
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
        public void Should_Schedule_Valid_Emergency_Job_Request_And_Send_Emergency_Job_Text_with_Date()
        {
            var request = new JobRequest{IsEmergency = true, RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = GetKnownTaskId()};
            var messenger = new FakeMessageSender();
            var sut = new UberJobProcessor(messenger);
            var result = sut.Process(request);
            result.ScheduledToBeginOn.ShouldEqual(request.RequestedByDate);
            var textMessage = (EmergencyJobScheduled) messenger.SentMessage;
            textMessage.ScheduledDate.ShouldEqual(request.RequestedByDate);
        }

        [TestMethod]
        public void Should_Schedule_Valid_Book_Job_Request_If_Requested_Date_At_Least_3_Days_In_The_Future()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(3), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.ScheduledToBeginOn.Date.ShouldEqual(request.RequestedByDate.Date);
        }

        [TestMethod]
        public void Should_Reject_Normal_Job_Request_If_Requested_Date_Sooner_Than_3_Days()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(2), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(RequestedDateTooSoonError)).ShouldBeTrue();
        }

        #region Existing Behaviors

        [TestMethod]
        public void Should_Schedule_Valid_Book_Job_Request_If_Scheduled_Work_Acceptable_for_Requested_Date()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(3), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.ScheduledToBeginOn.Date.ShouldEqual(request.RequestedByDate.Date);
        }

        [TestMethod]
        public void Should_Reject_Valid_Book_Job_Request_If_Requested_Date_Full()
        {
            var request = new JobRequest{RequestedByDate = _dateWithTooManyScheduledJobs, RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(RequestedDateFullError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Accept_Valid_Book_Job_Command()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.Accepted.ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_The_Past_With_Input_Errors()
        {
            var request = new JobRequest
                {RequestedByDate = DateTime.Now.Subtract(TimeSpan.FromDays(3)), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(JobInThePastError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_Unknown_Task_Id_With_UnknownJobError()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(3), RequestedTask = 1231354957};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(UnknownJobIdError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Schedule_Job_Based_On_Results()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = GetKnownTaskId()};
            var sut = new UberJobProcessor(new FakeMessageSender());
            var result = sut.Process(request);
            result.ScheduledToBeginOn.CompareTo(request.RequestedByDate).ShouldBeInRange(-14, 0);
        }

        #endregion Existing Behaviors
    }
}