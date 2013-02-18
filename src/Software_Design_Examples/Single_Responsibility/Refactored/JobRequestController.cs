using System.Collections.Generic;
using System.Linq;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class JobRequestController
    {
        private readonly EmergencyRequestValidator _emergencyRequestValidator;
        private readonly RegularRequestValidator _requestValidator;
        private readonly EmergencyJobScheduler _emergencyJobScheduler;
        private readonly JobScheduler _jobScheduler;

        public JobRequestController(EmergencyRequestValidator emergencyRequestValidator, RegularRequestValidator requestValidator, EmergencyJobScheduler emergencyJobScheduler, JobScheduler jobScheduler)
        {
            _emergencyRequestValidator = emergencyRequestValidator;
            _requestValidator = requestValidator;
            _emergencyJobScheduler = emergencyJobScheduler;
            _jobScheduler = jobScheduler;
        }

        public JobRequestResult Handle(JobRequest request)
        {
            var validationResult = request.IsEmergency
                                       ? _emergencyRequestValidator.Validate(request)
                                       : _requestValidator.Validate(request);
            if(!validationResult.IsValid)
            {
                var errors = new List<IJobRequestError>();
                if (validationResult.Errors != null && validationResult.Errors.Any()) errors = new List<IJobRequestError>(validationResult.Errors);
                return new JobRequestResult {Errors = errors};
            }
            var schedulingResult = request.IsEmergency
                                       ? _emergencyJobScheduler.Schedule(request)
                                       : _jobScheduler.Schedule(request);
            return new JobRequestResult
                {
                    Accepted = true, 
                    ScheduledToStart = schedulingResult.ScheduledToStart, 
                    Errors = new List<IJobRequestError>(schedulingResult.Errors)
                };
        }
    }
}