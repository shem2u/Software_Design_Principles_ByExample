using System;
using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class EmergencyRequestValidator
    {
         public JobRequestValidationResult Validate(JobRequest request)
         {
             if (DateTime.Now.CompareTo(request.RequestedByDate) > 0)
                 return new JobRequestValidationResult
                     {Errors = new List<IValidationErrors> {new DateInThePastValidationError()}};
             using (var context = new SharedDbContext())
             {
                 var matchingKnownJob = context.Job.Find(request.RequestedTask);
                 if (matchingKnownJob == null)
                     return new JobRequestValidationResult
                         {Errors = new List<IValidationErrors> {new UnknownJobValidationError()}};
             }
             return new JobRequestValidationResult{IsValid = true};
         }
    }
}