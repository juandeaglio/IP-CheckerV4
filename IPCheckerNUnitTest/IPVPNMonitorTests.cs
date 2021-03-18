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
        public IPMonitor ipMonitor;
        [SetUp]
        public void Setup()
        {
            ipMonitor = new IPMonitor();
            ipMonitor.UpdateWebsitesAction += (websites) => { };
            string temp;
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
    }
    public class VPNStabilityTests
    {
        public IPMonitor ipMonitor;
        public VPN_Stability_Monitor vpnMonitor;
        [SetUp]
        public void Setup()
        {
            ipMonitor = new IPMonitor();
            ipMonitor.UpdateWebsitesAction += (websites) => { };
            string temp;
            ipMonitor.UpdateIPFieldAction += (temp) => { };
            ipMonitor.SetWebsites(new HashSet<string>());
            vpnMonitor = new VPN_Stability_Monitor(ipMonitor);
            vpnMonitor.mi.HomeIP = "47.144.17.23";
            vpnMonitor.mi.VPNIP = "192.168.0.112";
            ipMonitor.currentIP = "192.168.0.812";
            vpnMonitor.UpdateStabilityAction += (websites) => { };
            vpnMonitor.active = true;
        }
        [TearDown]
        public void Teardown()
        {
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
            Assert.IsTrue(vpnMonitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyVPNIP2()
        {
            vpnMonitor.mi.VPNIP = "";
            ipMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(vpnMonitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldVerifyHomeIP()
        {
            ipMonitor.currentIP = "47.144.17.23";
            Assert.IsTrue(vpnMonitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP()
        {
            ipMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(vpnMonitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldVerifyHomeIP2()
        {
            vpnMonitor.mi.HomeIP = "";
            ipMonitor.currentIP = "172.54.43.22";
            Assert.IsTrue(vpnMonitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyHomeIP3()
        {
            ipMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(vpnMonitor.VerifyHomeIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP()
        {
            ipMonitor.currentIP = "4343.3434343.4343.4343.";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP2()
        {
            ipMonitor.currentIP = "172.245.212.22";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP());
        }
        [Test]
        public void ShouldNotVerifyVPNIP3()
        {
            ipMonitor.currentIP = "NE.43./4]fdez";
            Assert.IsFalse(vpnMonitor.VerifyVPNIP());
        }
    }
}