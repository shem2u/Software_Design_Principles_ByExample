using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Software_Design_Examples;

namespace Software_Design_Examples_Tests
{
    [TestClass]
    public class IntegratedDbTest
    {
        private const string SQL_COMPACT_CONNECTION_STRING = "SharedDbContextForTest.sdf";

        [AssemblyInitialize]
        public static void AssembyInit(TestContext unused)
        {
            WithDbContext(x => { x.Database.CreateIfNotExists(); x.Database.Initialize(true); });
            //ClearDatabaseForTest();
        }

        protected static void WithDbContext(Action<SharedDbContext> action)
        {
            if(action == null) return;
            using (var context = new SharedDbContext())
            {
                action(context);
            }
        }

        private static void ClearDatabaseForTest()
        {
            WithDbContext(x =>
                {
                    x.Database.ExecuteSqlCommand("delete from AvailableJob");
                    x.Database.ExecuteSqlCommand("delete from ScheduledJob");
                });
        }
    }
}