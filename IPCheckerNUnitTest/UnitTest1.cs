using NUnit.Framework;
using IP_Checker;
namespace IPCheckerNUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void ShouldHaveConnectionTrue()
        {
            Assert.IsTrue(IPMonitor.IsConnectionActive());
        }
    }
}