using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace IP_Checker
{
    [Serializable]
    public struct MonitorInformation
    {
        public MonitorInformation(string homeIP, string vpnIP)
        {
            HomeIP = homeIP;
            VPNIP = vpnIP;
        }
        public string HomeIP;
        public string VPNIP;
    }

    public static class VPN_Stability_Monitor
    {
        static string FILENAME = "IPFile.dat";
        private static int totalWarnings;
        private static int threshold = 5;
        private static bool thresholdReached = false;
        private static string Stability { get; set; }
        public static bool test = false;
        public static bool shutdown = false;
        public static MonitorInformation mi;
        static VPN_Stability_Monitor()
        {
            //reads HomeIP from previous uses, reads VPNIP from previous uses
            // else it gets both on its own.
            SessionInformationStorage sis = new SessionInformationStorage();
            try
            {
                mi = sis.Deserialize(FILENAME);
            }
            catch
            {
                //File DNE or corrupted.
                mi = new MonitorInformation();
            }
        }
        private static void SetShutdownThresholdCounter(int value)
        {
            //Do something if thresholdReached.
            if (totalWarnings > threshold)
                thresholdReached = true;
            else
                thresholdReached = false;
            totalWarnings = value;
        }
        static void VPNStabilityCheck()
        {
            if (IPMonitor.currentIP != null)
            {
                if (IsValidIP(IPMonitor.currentIP) && IsSimilarTo(IPMonitor.currentIP, mi.VPNIP, 3))
                    SetShutdownThresholdCounter(0);
                else if (IPMonitor.currentIP.Equals(mi.HomeIP) || !IsSimilarTo(IPMonitor.currentIP, mi.VPNIP, 3))
                    SetShutdownThresholdCounter(totalWarnings + 1);

                Stability = thresholdReached ? $"Connection unstable after {totalWarnings} tries." : $"VPN active {mi.VPNIP}, connection stable. Home IP: {mi.HomeIP}";
                UpdateStabilityAction(Stability);
            }
        }
        public static void Run()
        {
            //TODO: Check for torrent application whether its active or not and shutdown if on home IP at the same time.
            Stability = "Unknown, VPN inactive.";
            UpdateStabilityAction(Stability);
            while (true)
            {
                //Check if we're getting a good IP.
                if (IsValidIP(IPMonitor.currentIP))
                {
                    //Now let's see whether we are have our home IP (all is normal).
                    if (VerifyHomeIP())
                    {
                        //Now let's see whether we have our VPN IP (VPN is active, all is normal).
                        if (VerifyVPNIP())
                        {
                            //Should remain the same... I think it's ready to go (Now about to check for stability issues.)
                            if (VerifyVPNIP())
                            {
                                //Stability check, functionality for shutting down if it's not normal.
                                while (true)
                                {
                                    VPNStabilityCheck();
                                    if (totalWarnings > threshold)
                                        shutdown = true;
                                    Thread.Sleep(500);
                                }
                                //It broke!
                            }
                            Console.WriteLine("Aaah!");
                        }
                        else
                        {
                            Stability = $"HomeIP is active {mi.HomeIP}, VPNIP is NOT active. {mi.VPNIP}";
                            UpdateStabilityAction(Stability);
                        }
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        Stability = "Unknown, HomeIP is unknown.";
                        UpdateStabilityAction(Stability);
                    }
                }
                else
                    Stability = "Unknown, internet connection is offline.";
                UpdateStabilityAction(Stability);
            }
        }
        public static bool VerifyHomeIP()
        {
            if ((IPMonitor.currentIP.Equals(mi.VPNIP) || IsSimilarTo(IPMonitor.currentIP, mi.VPNIP, 9)) && !IsSimilarTo(mi.HomeIP, mi.VPNIP, 9) || IsValidIP(mi.HomeIP))
                return true;
            else if (IsValidIP(mi.HomeIP))
            {
                if( IPMonitor.currentIP.Equals(mi.HomeIP))
                    return true;
                else
                {
                    if (IsSimilarTo(mi.HomeIP, IPMonitor.currentIP, 9))
                    {
                        mi.HomeIP = IPMonitor.currentIP;
                        SessionInformationStorage sis = new SessionInformationStorage();
                        sis.Serialize(mi, FILENAME);
                        return true;
                    }
                    return false;
                }
            }
            else
                return false;
        }
        public static bool VerifyVPNIP()
        {
            /*if (test)
            {
                IPMonitor.currentIP = "126.44.36.226";
                IPMonitor.stop = true;
            }*/
            //If the VPN ip is valid, check if it is similar to our home IP, if it is reset VPN IP and false.
            if (IsValidIP(mi.VPNIP) && IsSimilarTo(mi.VPNIP, mi.HomeIP, 9))
            {
                //test = true;
                mi.VPNIP = "";
                return false;
            }
            //If the VPN IP is valid, and our current IP is equal to VPN ip.
            else if (IsValidIP(mi.VPNIP) && IsSimilarTo(IPMonitor.currentIP, mi.VPNIP, 3))
            {
                //test = true;
                return true;
            }
            //If the current IP is not equal to VPN IP...
            else if (!IPMonitor.currentIP.Equals(mi.VPNIP))
            {
                // Check if it is valid, then check if it is NOT similar to home IP
                if (IsValidIP(IPMonitor.currentIP) && !IsSimilarTo(IPMonitor.currentIP, mi.HomeIP, 9))
                {
                    //Set the VPNIP to whatever IPMonitor reads.
                    mi.VPNIP = IPMonitor.currentIP;
                    SessionInformationStorage sis = new SessionInformationStorage();
                    sis.Serialize(mi, FILENAME);
                    return true;
                }
                //test = true;
                return false;
            }
            return false;
        }
        public static bool IsValidIP(string ip)
        {
            if (ip != null && !ip.Equals("") && Regex.IsMatch(ip, @"\d*\.\d*\.\d*\.\d*"))
                return true;
            return false;
        }
        public static bool IsSimilarTo(string ip, string ip2, int length)
        {
            if (IsValidIP(ip) && IsValidIP(ip2) && ip.Substring(0, length).Equals(ip2.Substring(0, length)))
                return true;
            else
                return false;
        }
        public static bool VPNIsActive()
        {
            Process[] processArr;
            processArr = Process.GetProcessesByName("ExpressVPN");
            if (processArr.Length > 0)
                return true;
            processArr = Process.GetProcessesByName("OpenVPN");
            if (processArr.Length > 0)
                return true;
            return false;
        }
        public static Action<string> UpdateStabilityAction;
    }
}
