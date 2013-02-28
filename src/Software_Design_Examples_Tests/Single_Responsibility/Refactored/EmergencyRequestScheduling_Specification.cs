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
    /// only one reason to change - it has been decided that job scheduling 
    /// is a single responsibility (concern).  Further, it has been decided
    /// the scheduling of emergency vs regular requests represent seperate 
    /// responsibilities (concerns).
    /// 
    /// These tests specify the expectations around how to schedule an 
    /// emergency request.
    /// 
    /// An emergency request is always scheduled and a message should be
    /// sent with the requested date to Engineering Management.
    /// 
    /// 
    /// For context and a more detailed explanation of these decisions, please
    /// see the wiki (https://github.com/shem2u/Software_Design_Principles_ByExample/wiki/Single-Responsibility)
    /// </summary>
    [TestClass]
    public class EmergencyRequestScheduling_Specification : IntegratedDbTest
    {
        [ClassInitialize]
        public static void OnTestInit(TestContext unused)
        {
            //This isn't really necessary because the scheduler shouldn't validate the data but . . .
            WithDbContext(context =>
                {
                    var knownJob = context.Job.Create();
                    knownJob.Description = "Testing Emergency Request Scheduling Design";
                    context.Job.Add(knownJob);
                    context.SaveChanges();
                });
        }

        //This isn't really necessary because the scheduler shouldn't validate the data but . . .
        private int GetKnownTaskId()
        {
            var taskId = 0;
            WithDbContext(ctx => taskId = ctx.Job.First().Id);
            return taskId;
        }

        [TestMethod]
        public void Should_Schedule_Request_for_Requested_Date()
        {
            var request = new JobRequest
                {IsEmergency = true, RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = GetKnownTaskId()};
            var sut = new EmergencyJobScheduler(new FakeMessageSender());
            var result = sut.Schedule(request);
            result.ScheduledToStart.ShouldEqual(request.RequestedByDate);
        }

        [TestMethod]
        public void Should_Send_Message_Including_Scheduled_Date()
        {
            var sender = new FakeMessageSender();
            var request = new JobRequest
                {IsEmergency = true, RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = GetKnownTaskId()};
            var sut = new EmergencyJobScheduler(sender);
            sut.Schedule(request);
            var message = sender.SentMessage as EmergencyJobScheduled;
            message.ScheduledOn.ShouldEqual(request.RequestedByDate);
        }
    }
}