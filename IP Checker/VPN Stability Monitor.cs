using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace IP_Checker
{
    [Serializable]
    public struct MonitorInformation
    {
        public MonitorInformation(string homeIP, string vpnIP, string currentIP)
        {
            HomeIP = homeIP;
            VPNIP = vpnIP;
            CurrentIP = currentIP;
        }
        public string HomeIP;
        public string VPNIP;
        public string CurrentIP;
    }

    public  class VPN_Stability_Monitor
    {
        string FILENAME = "IPFile.dat";
        private int totalWarnings;
        private int threshold = 5;
        private  bool thresholdReached = false;
        private string Stability { get; set; }
        public bool test = false;
        public bool shutdown = false;
        public MonitorInformation mi;
        public bool active = true;
        public IPMonitor ipMonitor;
        public VPN_Stability_Monitor(IPMonitor ipMon)
        {
            SessionInformationStorage sis = new SessionInformationStorage();
            try
            {
                mi = sis.Deserialize(FILENAME, this);
                ipMonitor = ipMon;
            }
            catch
            {
                mi = new MonitorInformation();
            }
        }
        private  void SetShutdownThresholdCounter(int value)
        {
            //Do something if thresholdReached.
            if (totalWarnings > threshold)
                thresholdReached = true;
            else
                thresholdReached = false;
            totalWarnings = value;
        }
         void IncrementThresholdIfUnstable(MonitorInformation mi)
        {
            if (mi.CurrentIP != null)
            {
                if (IsValidIP(mi.CurrentIP) && IsSimilarTo(mi.CurrentIP, mi.VPNIP, 3))
                    SetShutdownThresholdCounter(0);
                else if (mi.CurrentIP.Equals(mi.HomeIP) || !IsSimilarTo(mi.CurrentIP, mi.VPNIP, 3))
                    SetShutdownThresholdCounter(totalWarnings + 1);
                Stability = thresholdReached ? $"Connection unstable after {totalWarnings} tries." : $"VPN active {mi.VPNIP}, connection stable. Home IP: {mi.HomeIP}";
            }
        }
        public  void Run()
        {
            do { UpdateStabilityAction(Stability); } while (!AwaitSuccessfulVPNStart());
            active = true;
            UpdateStabilityAction(Stability);
            while (active)
            {
                active = VerifyStability();
                UpdateStabilityAction(Stability);
            }
            //shutdown = true;
        }

        public  bool VerifyStability()
        {
            if(IsStillActive())
            {
                return true;
            }
            return false;
        }

        private  bool AwaitSuccessfulVPNStart()
        {
            bool result = false;
            mi.CurrentIP = ipMonitor.currentIP;
            if (IsValidIP(mi.CurrentIP))
                if (VerifyHomeIP(mi))
                {
                    if (VerifyVPNIP(mi))
                    {
                        if (VerifyVPNIP(mi))
                            return true;
                    }
                    else
                    {
                        Stability = $"HomeIP is active {mi.HomeIP}, VPNIP is NOT active. {mi.VPNIP}";
                    }
                }
                else
                {
                    Stability = "Unknown, HomeIP is unknown.";
                }
            else
                Stability = "Unknown, internet connection is offline.";

            return result;
        }

        private  bool IsStillActive()
        {
            mi.CurrentIP = ipMonitor.currentIP;
            IncrementThresholdIfUnstable(mi); //needs renaming.
            if (totalWarnings > threshold)
                return false;
            else
                return true;
        }

        public  bool VerifyHomeIP(MonitorInformation mi)
        {
            if (IsSimilarTo(mi.CurrentIP, mi.VPNIP, 9) && !IsSimilarTo(mi.HomeIP, mi.VPNIP, 9))
                return true;
            else if (IsValidIP(mi.HomeIP))
            {
                if (mi.CurrentIP.Equals(mi.HomeIP))
                    return true;
                else
                {
                    if (IsSimilarTo(mi.HomeIP, mi.CurrentIP, 9))
                    {
                        mi.HomeIP = mi.CurrentIP;
                        SessionInformationStorage sis = new SessionInformationStorage();
                        sis.Serialize(mi, FILENAME);
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (IsValidIP(mi.CurrentIP))
                {
                    mi.HomeIP = mi.CurrentIP;
                    SessionInformationStorage sis = new SessionInformationStorage();
                    try
                    {
                        sis.Serialize(mi, FILENAME);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Was unable to access file: " + e.Message);
                    }
                    return true;
                }
                else
                    return false;
            }
        }
        public  bool VerifyVPNIP(MonitorInformation mi)
        {
            if (IsValidIP(mi.VPNIP))
            {
                if (IsSimilarTo(mi.VPNIP, mi.HomeIP, 9))
                {
                    mi.VPNIP = "";
                    return false;
                }
                else if (IsSimilarTo(mi.CurrentIP, mi.VPNIP, 3))
                {
                    return true;
                }
                else
                    return false;
            }
            else if (!mi.CurrentIP.Equals(mi.HomeIP) && IsValidIP(mi.CurrentIP))
            {
                mi.VPNIP = mi.CurrentIP;
                SessionInformationStorage sis = new SessionInformationStorage();
                try
                {
                    sis.Serialize(mi, FILENAME);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to access file: " + e.Message);
                }
                return true;
            }
            else
            {
                if (!IsSimilarTo(mi.CurrentIP, mi.HomeIP, 9))
                {
                    mi.VPNIP = mi.CurrentIP;
                    SessionInformationStorage sis = new SessionInformationStorage();
                    try
                    {
                        sis.Serialize(mi, FILENAME);
                    }
                    catch(IOException e)
                    {
                        Console.WriteLine("Failed to access file: " + e.Message);
                    }
                    return true;
                }
                else
                    return false;
            }
        }
        public  bool IsValidIP(string ip)
        {
            if (ip != null && !ip.Equals("") && Regex.IsMatch(ip, @"\d*\.\d*\.\d*\.\d*"))
                return true;
            else
                return false;
        }
        public  bool IsSimilarTo(string ip, string ip2, int length)
        {
            if (Math.Abs(ip.Length - ip2.Length) > 3)
                return false;
            else if (IsValidIP(ip) && IsValidIP(ip2) && ip.Substring(0, length).Equals(ip2.Substring(0, length)))
                return true;
            else
                return false;
        }
        public  bool VPNIsActive()
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
        public  Action<string> UpdateStabilityAction;
    }
}
