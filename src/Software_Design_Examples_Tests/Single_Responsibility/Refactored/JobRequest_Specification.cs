using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Software_Design_Examples.Single_Responsibility.Refactored;

namespace Software_Design_Examples_Tests.Single_Responsibility.Refactored
{
    /// <summary>
    /// As part of refactoring the existing solution in Feature_Change_2 to
    /// follow the Principle of Single Responsibility - A class should have
    /// only one reason to change - the responsibilities for validation and
    /// job scheduling have been moved into separate classes with classes for
    /// emergency and regular job request handling.
    /// After this refactoring, the system is missing the flow of validation
    /// followed by scheduling that existed in the initial implementation.
    /// 
    /// This class specifies that control flow and the system behavior within
    /// it.  It is a (sub)system test not a Unit Test.
    /// 
    /// The system should reject any requests that fail validation and include
    /// any errors.  It should schedule all valid requests and return the results
    /// (including any errors).
    /// 
    /// For context and a more detailed explanation of these decisions, please
    /// see the wiki ()
    /// </summary>
    [TestClass]
    public class JobRequest_Specification : IntegratedDbTest
    {
        private static DateTime _dateWithTooManyScheduledJobs = DateTime.Now.AddDays(4);
        
        [ClassInitialize]
        public static void OnTestInit(TestContext unused)
        {
            WithDbContext(context =>
                {
                    var knownJob = context.Job.Create();
                    knownJob.Description = "Testing Job Scheduling Design";
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
        public void Should_Reject_An_Invalid_Request()
        {
            var sender = new FakeMessageSender();
            var unkownTaskRequst = new JobRequest
                {IsEmergency = false, RequestedByDate = DateTime.Now.AddDays(5), RequestedTask = 12345678};
            var sut = new JobRequestController(new EmergencyRequestValidator(), new RegularRequestValidator(), new EmergencyJobScheduler(sender), new JobScheduler());
            var result = sut.Handle(unkownTaskRequst);
            result.Accepted.ShouldBeFalse();
            result.Errors.Any(x => x.GetType() == typeof (UnknownJobValidationError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Accept_And_Schedule_A_Valid_Request()
        {
            var sender = new FakeMessageSender();
            var regularRequestForFullDate = new JobRequest
                {IsEmergency = false, RequestedByDate = _dateWithTooManyScheduledJobs, RequestedTask = GetKnownTaskId()};
            var sut = new JobRequestController(new EmergencyRequestValidator(), new RegularRequestValidator(), new EmergencyJobScheduler(sender), new JobScheduler());
            var result = sut.Handle(regularRequestForFullDate);
            result.Accepted.ShouldBeTrue();
            result.ScheduledToStart.ShouldEqual(regularRequestForFullDate.RequestedByDate.AddDays(1));
        }

        [TestMethod]
        public void Should_Accept_And_Schedule_A_Valid_Emergency_Request()
        {
            var sender = new FakeMessageSender();
            var emergencyRequest = new JobRequest
                {IsEmergency = true, RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = GetKnownTaskId()};
            var sut = new JobRequestController(new EmergencyRequestValidator(), new RegularRequestValidator(), new EmergencyJobScheduler(sender), new JobScheduler());
            var result = sut.Handle(emergencyRequest);
            result.Accepted.ShouldBeTrue();
            result.ScheduledToStart.ShouldEqual(emergencyRequest.RequestedByDate);
        }
    }
}