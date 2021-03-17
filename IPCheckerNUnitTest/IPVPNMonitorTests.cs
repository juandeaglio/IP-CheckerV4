using IP_Checker;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace IPCheckerNUnitTest
{
    public class IPVPNMonitorUnitTests
    {
        public const string WEBSITE1 = "http://icanhazip.com";
        public const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        [SetUp]
        public void Setup()
        {
            IPMonitor.UpdateWebsitesAction += (websites) => { };
            IPMonitor.SetWebsites(new HashSet<string>());
        }
        //TODO: Change the add/remove website tests such that they do not interact w/ WebClient, but instead through some stub.
        [Test]
        public void ShouldAddWebsite()
        {
            IPMonitor.GetWebsites().Add(WEBSITE1);
            Assert.IsTrue(WebsitesAreNotEmpty(IPMonitor.GetWebsites()));
            Assert.GreaterOrEqual(IPMonitor.GetWebsites().Count, 1);
        }
        [Test]
        public void ShouldRemoveWebsite()
        {
            IPMonitor.GetWebsites().Add(WEBSITE1);
            Assert.Greater(IPMonitor.GetWebsites().Count, 0);
            IPMonitor.GetWebsites().Remove(WEBSITE1);
            Assert.AreEqual(0, IPMonitor.GetWebsites().Count);
        }
        [Test]
        public void ShouldAddDuplicateWebsite()
        {
            IPMonitor.GetWebsites().Add(WEBSITE1);
            IPMonitor.GetWebsites().Add(WEBSITE1);
            Assert.AreEqual(IPMonitor.GetWebsites().Count, 1);
        }
        [Test]
        public void ShouldNotHaveConnection()
        {
            IPMonitor.GetWebsites().Add("Ht:/dwewe");
            Assert.IsFalse(IPMonitor.IsConnectionActive());
        }
        public bool WebsitesAreNotEmpty(HashSet<string> websites)
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
            Assert.IsFalse(IPMonitor.TryFetchIP(""));
        }
    }
    public class VPNStabilityTests
    {
        [SetUp]
        public void Setup()
        {
            VPN_Stability_Monitor.mi.HomeIP = "47.144.17.23";
            VPN_Stability_Monitor.mi.VPNIP = "192.168.0.112";
            IPMonitor.currentIP = "192.168.0.812";
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
            Assert.IsTrue(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyVPNIP2()
        {
            VPN_Stability_Monitor.mi.VPNIP = "";
            IPMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyHomeIP()
        {
            IPMonitor.currentIP = "47.144.17.23";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP()
        {
            IPMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldVerifyHomeIP2()
        {
            VPN_Stability_Monitor.mi.HomeIP = "";
            IPMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP3()
        {
            IPMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP()
        {
            IPMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP2()
        {
            IPMonitor.currentIP = "172.245.212.22";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP3()
        {
            IPMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(VPN_Stability_Monitor.VerifyVPNIP());
        }
    }
}