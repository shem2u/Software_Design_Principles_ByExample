using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    /// For context and a more detailed explanation of these decisions, please
    /// see the wiki ()
    /// </summary>
    [TestClass]
    public class EmergencyRequestValidation_Specification : IntegratedDbTest
    {
    }
}