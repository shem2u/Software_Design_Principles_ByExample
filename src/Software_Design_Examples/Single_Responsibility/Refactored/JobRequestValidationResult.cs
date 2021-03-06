using System.Collections.Generic;

namespace Software_Design_Examples.Single_Responsibility.Refactored
{
    public class JobRequestValidationResult
    {
        public IEnumerable<IValidationError> Errors { get; set; }
        public bool IsValid { get; set; }
    }
}