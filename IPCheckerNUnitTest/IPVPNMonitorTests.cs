using IP_Checker;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace IPCheckerNUnitTest
{
    public class TestWebsiteTester : WebsiteTester
    {

        public TestWebsiteTester(HashSet<string> set) : base(set)
        {

        }
        public override void Add(string name)
        {
            HashSet<string> set = base.GetWebsiteSet();
            if (!base.GetWebsiteSet().Contains(name))
                set.Add(name);
        }
    }
    public class IPVPNMonitorUnitTests
    {
        public const string WEBSITE1 = "http://icanhazip.com";
        public const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        public WebsiteTester websiteTester;
        public IPMonitor ipMonitor;

        [SetUp]
        public void Setup()
        {
            websiteTester = new TestWebsiteTester(new HashSet<string>());
            ipMonitor = new IPMonitor(websiteTester);
            ipMonitor.UpdateWebsitesAction += (websites) => { };
            ipMonitor.UpdateIPFieldAction += (temp) => { };
        }
        [Test]
        public void GivenAnInvalidWebsiteShouldNotHaveConnection()
        {
            ipMonitor.AddWebsite("Ht:/dwewe");
            Assert.IsFalse(ipMonitor.IsConnectionActive());
        }
        [Test]
        public void GivenWebsitesIsEmptyShouldNotFetchIP()
        {
            Assert.IsFalse(ipMonitor.TryFetchIP(""));
        }
        [Test]
        public void ShouldAddOneWebsite()
        {
            int sizeOfSet = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE3);
            Assert.AreEqual(sizeOfSet + 1, ipMonitor.GetWebsites().Count);
        }
        [Test]
        public void ShouldAddOneWebsiteThenAddDuplicateWebsite()
        {
            int sizeOfSet = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE3);
            ipMonitor.AddWebsite(WEBSITE3);
            Assert.AreEqual(sizeOfSet + 1, ipMonitor.GetWebsites().Count);
        }
        [Test]
        public void GivenSetWithOneWebsiteShouldDeleteOneWebsite()
        {
            int sizeOfSet = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE3);
            ipMonitor.RemoveWebsite(WEBSITE3);
            Assert.AreEqual(sizeOfSet, ipMonitor.GetWebsites().Count);
        }
    }
    public class VPNStabilityUnitTests
    {
        public IPMonitor ipMonitor;
        public WebsiteTester websiteTester;
        public VPN_Stability_Monitor vpnMonitor;
        string homeIP;
        string vpnIP;
        string currentIP;
        MonitorInformation mi;
        [SetUp]
        public void Setup()
        {
            websiteTester = new WebsiteTester(new HashSet<string>());
            ipMonitor = new IPMonitor(websiteTester);
            ipMonitor.UpdateWebsitesAction += (websites) => { };
            string temp;
            ipMonitor.UpdateIPFieldAction += (temp) => { };
            ipMonitor.SetWebsites(new HashSet<string>());
            vpnMonitor = new VPN_Stability_Monitor(ipMonitor);
            mi = new MonitorInformation("47.144.17.23", "192.168.0.112", "192.168.0.812");
            vpnMonitor.UpdateStabilityAction += (websites) => { };
            vpnMonitor.active = true;
        }
        [Test]
        public void ShouldHaveStableVPN()
        {
            Thread run = new Thread(() =>
             {
                 vpnMonitor.Run();
             });
            run.Start();
            Assert.IsTrue(vpnMonitor.VerifyStability());
        }
        [Test]
        public void ShouldVerifyVPNIP()
        {
            Assert.IsTrue(vpnMonitor.VerifyVPNIP(mi));
        }
        [Test]
        public void ShouldVerifyVPNIP2()
        {
            mi.VPNIP = "";
            mi.CurrentIP = "172.54.43.22";
            Assert.IsTrue(vpnMonitor.VerifyVPNIP(mi));
        }
        [Test]
        public void ShouldVerifyHomeIP()
        {
            mi.CurrentIP = "47.144.17.23";
            Assert.IsTrue(vpnMonitor.VerifyHomeIP(mi));
        }
        [Test]
        public void ShouldNotVerifyHomeIP()
        {
            mi.CurrentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(vpnMonitor.VerifyHomeIP(mi));
        }
        [Test]
        public void ShouldVerifyHomeIP2()
        {
            mi.HomeIP = "";
            mi.CurrentIP = "172.54.43.22";
            Assert.IsTrue(vpnMonitor.VerifyHomeIP(mi));
        }
        [Test]
        public void ShouldNotVerifyHomeIP3()
        {
            mi.CurrentIP = "NE.43./4]fdez";
            Assert.IsFalse(vpnMonitor.VerifyHomeIP(mi));
        }
        [Test]
        public void ShouldNotVerifyVPNIP()
        {
            mi.CurrentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP(mi));
        }
        [Test]
        public void ShouldNotVerifyVPNIP2()
        {
            mi.CurrentIP = "172.245.212.22";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP(mi));
        }
        [Test]
        public void ShouldNotVerifyVPNIP3()
        {
            mi.CurrentIP = "NE.43./4]fdez";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP(mi));
        }
    }
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
            Assert.AreEqual(amntOfWebsites+1, ipMonitor.GetWebsites().Count);
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
        public void ShouldAddWebsiteThenHaveAConnection()
        {
            int amntOfWebsites = ipMonitor.GetWebsites().Count;
            ipMonitor.AddWebsite(WEBSITE1);
            Assert.AreEqual(amntOfWebsites + 1, ipMonitor.GetWebsites().Count);
            Assert.IsTrue(ipMonitor.IsConnectionActive());
        }
    }
}