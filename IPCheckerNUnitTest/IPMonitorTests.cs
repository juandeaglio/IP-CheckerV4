using IP_Checker;
using NUnit.Framework;
namespace IPCheckerNUnitTest
{
    public class Tests
    {
        public const string WEBSITE1 = "http://icanhazip.com";
        public const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        [TearDown]
        public void TearDown()
        {
            IPMonitor.websites.Clear();
        }
        [SetUp]
        public void Setup()
        {
            IPMonitor.UpdateWebsitesAction += (websites) => { };
        }
        [Test]
        public void ShouldAddWebsite()
        {
            IPMonitor.AddWebsite(WEBSITE1);
            Assert.IsTrue(WebsitesAreNotEmpty(IPMonitor.websites));
            Assert.GreaterOrEqual(IPMonitor.websites.Count, 1);
        }
        [Test]
        public void ShouldRemoveWebsite()
        {
            IPMonitor.AddWebsite(WEBSITE1);
            Assert.Greater(IPMonitor.websites.Count, 0);
            IPMonitor.RemoveWebsite(WEBSITE1);
            Assert.AreEqual(0, IPMonitor.websites.Count);
        }
        [Test]
        public void ShouldAddDuplicateWebsite()
        {
            IPMonitor.AddWebsite(WEBSITE1);
            IPMonitor.AddWebsite(WEBSITE1);
            Assert.AreEqual(IPMonitor.websites.Count, 1);
        }
        [Test]
        public void ShouldAddThreeWebsites()
        {
            IPMonitor.AddWebsite(WEBSITE1);
            IPMonitor.AddWebsite(WEBSITE2);
            IPMonitor.AddWebsite(WEBSITE3);
            Assert.IsTrue(WebsitesAreNotEmpty(IPMonitor.websites));
            Assert.AreEqual(IPMonitor.websites.Count, 3);
        }
        [Test]
        public void ShouldHaveConnectionTrue()
        {

            IPMonitor.AddWebsite(WEBSITE1);
            Assert.IsTrue(IPMonitor.IsConnectionActive());
        }
        public bool WebsitesAreNotEmpty(WebsiteHashSet websites)
        {
            foreach (string website in websites)
            {
                if (string.IsNullOrEmpty(website))
                    return false;
            }
            return true;
        }
        [Test]
        public void ShouldHaveNoConnection()
        {
            Assert.IsFalse(IPMonitor.IsConnectionActive());

        }
    }
}