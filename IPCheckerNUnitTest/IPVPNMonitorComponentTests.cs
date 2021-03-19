using IP_Checker;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;


namespace IPCheckerComponentTests
{
    public class IPMonitorComponentTests
    {
        public const string WEBSITE1 = "http://icanhazip.com";
        public const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";

        public IPMonitor ipMonitor;
        public WebsiteTester websiteTester;
        [SetUp]
        public void Setup()
        {
            HashSet<string> websites = new HashSet<string>();
            WebsiteTester websiteTester = new WebsiteTester(websites);
            ipMonitor = new IPMonitor(websiteTester);
            ipMonitor.UpdateWebsitesAction += (websites) => { };
            string temp;
            ipMonitor.UpdateIPFieldAction += (temp) => { };
        }
        [Test]
        public void ShouldAddOneValidWebsite()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            Assert.AreEqual(amntOfWebsites + 1, ipMonitor.GetWebsites().Count);
        }
        [Test]
        public void ShouldAddOneValidWebsiteThenTryAddingDuplicateWebsite()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            ipMonitor.AddWebsite(WEBSITE1);
            Assert.AreEqual(amntOfWebsites + 1, ipMonitor.GetWebsites().Count);
        }
        [Test]
        public void ShouldAddOneValidWebsiteThenRemoveIt()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            ipMonitor.RemoveWebsite(WEBSITE1);
            Assert.AreEqual(amntOfWebsites, ipMonitor.GetWebsites().Count);
        }
        [Test]
        public void GivenEmptyWebsitesShouldNotHaveAConnection()
        {
            Assert.IsFalse(ipMonitor.IsConnectionActive());
        }
        [Test]
        public void ShouldAddWebsiteThenVerifyConnection()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            Assert.AreEqual(amntOfWebsites + 1, ipMonitor.GetWebsites().Count);
            Assert.IsTrue(ipMonitor.IsConnectionActive());
        }
        [Test]
        public void ShouldAddMultipleWebsitesThenVerifyConnection()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            ipMonitor.AddWebsite(WEBSITE2);
            ipMonitor.AddWebsite(WEBSITE3);
            Assert.AreEqual(amntOfWebsites + 3, ipMonitor.GetWebsites().Count);
            Assert.IsTrue(ipMonitor.IsConnectionActive());
        }
        [Test]
        public void GivenMultipleWebsitesShouldFindFastestOne()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            bool test = false;
            ipMonitor.AddWebsite(WEBSITE1);
            ipMonitor.AddWebsite(WEBSITE2);
            ipMonitor.AddWebsite(WEBSITE3);
            ipMonitor.IsConnectionActive();
            Assert.AreEqual(amntOfWebsites + 3, ipMonitor.GetWebsites().Count);
            Assert.IsTrue(ipMonitor.GetWebsites().Contains(ipMonitor.FastestWebsite));
        }
    }
}
