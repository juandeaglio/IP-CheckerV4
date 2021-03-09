using IP_Checker;
using NUnit.Framework;
using System.Threading;

namespace IPCheckerNUnitTest
{
    public class IPMonitorTests
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
        public void ShouldNotHaveConnection()
        {
            IPMonitor.AddWebsite("Ht:/dwewe");
            Assert.IsFalse(IPMonitor.IsConnectionActive());
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
        public void ShouldNotFetchIPWhenWebsitesEmpty()
        {
            Assert.IsFalse(IPMonitor.TryFetchIP());
        }
    }
    public class VPNStabilityTests
    {
        [SetUp]
        public void Setup()
        {
            VPN_Stability_Monitor.mi.HomeIP = "";
            VPN_Stability_Monitor.mi.VPNIP = "";
            VPN_Stability_Monitor.UpdateStabilityAction += (websites) => { };
            VPN_Stability_Monitor.active = true;
        }
        [TearDown]
        public void Teardown()
        {
            VPN_Stability_Monitor.active = false;
        }
        [Test]
        public void ShouldHaveStableVPN()
        {
            Thread run = new Thread(() =>
             {
                 VPN_Stability_Monitor.Run();
             });
            run.Start();
            Assert.IsTrue(VPN_Stability_Monitor.VerifyStability());
            VPN_Stability_Monitor.active = false;
        }
        [Test]
        public void ShouldVerifyVPNIP()
        {
            VPN_Stability_Monitor.mi.HomeIP = "47.144.17.23";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "192.168.0.812";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyVPNIP2()
        {
            VPN_Stability_Monitor.mi.HomeIP = "47.144.17.23";
            VPN_Stability_Monitor.mi.VPNIP = "";
            IPMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyHomeIP()
        {
            VPN_Stability_Monitor.mi.HomeIP = "47.144.17.23";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "47.144.17.23";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP()
        {
            VPN_Stability_Monitor.mi.HomeIP = "34.343.232.12";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldVerifyHomeIP2()
        {
            VPN_Stability_Monitor.mi.HomeIP = "";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP3()
        {
            VPN_Stability_Monitor.mi.HomeIP = "34.343.232.12";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP()
        {
            VPN_Stability_Monitor.mi.HomeIP = "34.343.232.12";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP2()
        {
            VPN_Stability_Monitor.mi.HomeIP = "34.343.232.12";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "172.245.212.22";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP3()
        {
            VPN_Stability_Monitor.mi.HomeIP = "34.343.232.12";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
    }
}