using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Checker
{
    static class VPN_Stability_Monitor
    {
        private static int totalWarnings;
        private static int threshold = 5;
        private static bool thresholdReached = false;
        private static string HomeIP { get; set; }
        private static string VPNIP { get; set; }
        static VPN_Stability_Monitor()
        {
            
        }
        private static int ShutdownThresholdCounter
        {
            get => totalWarnings;
            set
            {
                //Do something if thresholdReached.
                if (totalWarnings > threshold && !thresholdReached)
                    thresholdReached = true;
            }
        }
        static void StabilityCheck()
        {
            if(!IPMonitor.currentIP.Equals(HomeIP) && IsValidIP(IPMonitor.currentIP))
            {
                ShutdownThresholdCounter = 0;
            }
            else if(IPMonitor.currentIP.Equals(HomeIP))
            {

            }
        }
        public static void Run()
        {

        }
        static bool IsValidIP(string ip)
        {
            return true;
        }
    }
}
