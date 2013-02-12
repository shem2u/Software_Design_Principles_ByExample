using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using Software_Design_Examples.Single_Responsibility.An_Initial_Design;

namespace Software_Design_Examples_Tests.Single_Responsibility
{
    /// <summary>
    /// This is an example of a class violating the Single Responsibility Principle
    /// The tests below expose two orthoginal concerns (responsibilities)
    /// 
    /// *** Context ***
    /// This is intended to support a job scheduling feature of larger facilities 
    /// management suite.  The intent is to allow building administrators to request
    /// jobs to be performed via the tool.  It should validate the job can be 
    /// performed and then schedule the work with an available engineer.
    /// 
    /// As a Building Administrator
    /// I want to request an engineer perform a task
    /// So that I can ensure my building functions as required
    /// 
    /// Given an Administrator with a task to be performed
    /// And the task exists in the system
    /// And the task does not have a requested due date in the past
    /// When the Administrator submits the request
    /// Then the system should accept the request
    /// And schedule an engineer to begin the job on the first available date
    /// 
    /// See wiki ( http:// ) for details
    /// </summary>
    [TestClass]
    public class An_Initial_Design_Specification
    {
        [TestMethod]
        public void Should_Execute_Valid_Book_Job_Command()
        {
            var request = new JobRequest{RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = 12345};
            var sut = new JobProcessor();
            var result = sut.Process(request);
            result.Accepted.ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_The_Past_With_Input_Errors()
        {
            var request = new JobRequest
                {RequestedByDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)), RequestedTask = 12345};
            var sut = new JobProcessor();
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(JobInThePastError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Reject_Book_Job_Command_For_Unknown_Task_Id_With_UnknownJobError()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(1), RequestedTask = 666};
            var sut = new JobProcessor();
            var result = sut.Process(request);
            result.Errors.Any(x => x.GetType() == typeof(UnknownJobIdError)).ShouldBeTrue();
        }

        [TestMethod]
        public void Should_Schedule_Job_Based_On_Results()
        {
            var request = new JobRequest {RequestedByDate = DateTime.Now.AddDays(14), RequestedTask = 12345};
            var sut = new JobProcessor();
            var result = sut.Process(request);
            result.ScheduledToBeginOn.CompareTo(request.RequestedByDate).ShouldBeInRange(-14, 0);
        }
    }
}