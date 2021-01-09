using NUnit.Framework;
using IP_Checker;
namespace IPCheckerNUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            IPMonitor.UpdateWebsitesAction += (websites) => { };
        }
        [Test]
        public void ShouldAddWebsite()
        {
            IPMonitor.AddWebsite("http://icanhazip.com");
            foreach(string website in IPMonitor.websites)
            {
                Assert.IsNotEmpty(website);
            }
            
        }
        [Test]
        public void ShouldHaveConnectionTrue()
        {

            IPMonitor.AddWebsite("http://icanhazip.com");
            Assert.IsTrue(IPMonitor.IsConnectionActive());
        }
    }
}