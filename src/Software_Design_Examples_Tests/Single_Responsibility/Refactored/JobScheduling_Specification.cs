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
    /// These tests specify the expectations around how to schedule a simple
    /// job request.
    /// 
    /// A job request is always scheduled for the requested date unless there
    /// are already 4 jobs scheduled for that day.  In that case, it is 
    /// scheduled for the next available date.
    /// 
    /// For context and a more detailed explanation of these decisions, please
    /// see the wiki (https://github.com/shem2u/Software_Design_Principles_ByExample/wiki/Single-Responsibility)
    /// </summary>
    [TestClass]
    public class JobScheduling_Specification : IntegratedDbTest
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
        public void Should_Schedule_Job_For_Requested_Date_If_Less_Than_4_Jobs_Scheduled_That_Date()
        {
            var request = new JobRequest
                {IsEmergency = false, RequestedByDate = DateTime.Now.AddDays(10), RequestedTask = GetKnownTaskId()};
            var sut = new JobScheduler();
            var result = sut.Schedule(request);
            result.ScheduledToStart.ShouldEqual(request.RequestedByDate);
        }

        [TestMethod]
        public void Should_Schedule_Job_For_Next_Available_Date_If_4_Jobs_Scheduled_On_Requested_Date()
        {
            var request = new JobRequest
                {IsEmergency = false, RequestedByDate = _dateWithTooManyScheduledJobs, RequestedTask = GetKnownTaskId()};
            var sut = new JobScheduler();
            var result = sut.Schedule(request);
            result.ScheduledToStart.ShouldEqual(request.RequestedByDate.AddDays(1));
            result.Errors.Any(x => x.GetType() == typeof (RequestedDateFullError)).ShouldBeTrue();
        }
    }
}