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
    /// only one reason to change - it has been decided that request validation
    /// is a single responsibility (concern).  Further, it has been decided that
    /// validation of emergency vs regular requests represent seperate 
    /// responsibilities (concerns).
    /// 
    /// These tests specify the expectations around what consistutes a valid 
    /// regular request.
    /// 
    /// A job request must be made for a known task type and must not
    /// have a date in the past or earlier than 4 days from the current date.
    /// 
    /// For context and a more detailed explanation of these decisions, please
    /// see the wiki ()
    /// </summary>
    [TestClass]
    public class RegularRequestValidation_Specification : IntegratedDbTest
    {
        [ClassInitialize]
        public static void OnTestInit(TestContext unused)
        {
            WithDbContext(context =>
                {
                    var knownJob = context.Job.Create();
                    knownJob.Description = "Testing Regular Request Validation Design";
                    context.Job.Add(knownJob);
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
        public void Should_Reject_Request_For_Date_In_The_Past()
        {
            var request = new JobRequest
                {
                    IsEmergency = false,
                    RequestedByDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                    RequestedTask = GetKnownTaskId()
                };
            var sut = new RegularRequestValidator();
            var result = sut.Validate(request);
            result.Errors.Any(x => x.GetType() == typeof (DateInThePastValidationError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Request_For_Date_Sooner_Than_4_Days_From_Current_Date()
        {
            var request = new JobRequest
                {IsEmergency = false, RequestedByDate = DateTime.Now.AddDays(3), RequestedTask = GetKnownTaskId()};
            var sut = new RegularRequestValidator();
            var result = sut.Validate(request);
            result.Errors.Any(x => x.GetType() == typeof(DateTooSoonForNonEmergencyError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Request_For_Unknown_Task()
        {
            var request = new JobRequest
                {IsEmergency = false, RequestedByDate = DateTime.Now.AddDays(4), RequestedTask = 1234567};
            var sut = new RegularRequestValidator();
            var result = sut.Validate(request);
            result.Errors.Any(x => x.GetType() == typeof(UnknownJobValidationError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Accept_all_Other_Requests()
        {
            var request = new JobRequest
                {IsEmergency = false, RequestedByDate = DateTime.Now.AddDays(4), RequestedTask = GetKnownTaskId()};
            var sut = new RegularRequestValidator();
            var result = sut.Validate(request);
            result.IsValid.ShouldBeTrue();
        }
    }
}