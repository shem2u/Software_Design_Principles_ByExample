using System;
using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    /// <summary>
    /// Validates regular job requests to ensure they can be accepted
    /// </summary>
    public class RegularRequestValidator
    {
        public JobRequestValidationResult Validate(JobRequest request)
        {
            //Date cannot be in the past
            if (request.RequestedByDate.CompareTo(DateTime.Now) < 0)
                return new JobRequestValidationResult
                    {Errors = new List<IValidationError> {new DateInThePastValidationError()}};
            //Date cannot be earlier than 4 days from now
            if (request.RequestedByDate.CompareTo(DateTime.Now.AddDays(3)) <= 0)
                return new JobRequestValidationResult
                    {Errors = new List<IValidationError> {new DateTooSoonForNonEmergencyError()}};
            using (var context = new SharedDbContext())
            {
                var matchingKnownJob = context.Job.Find(request.RequestedTask);
                //must be known task
                if (matchingKnownJob == null)
                    return new JobRequestValidationResult
                        {Errors = new List<IValidationError> {new UnknownJobValidationError()}};
            }
            return new JobRequestValidationResult{IsValid = true, Errors = new List<IValidationError>()};
        }
    }
}