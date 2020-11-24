using System;
using System.Net;


namespace IP_Checker
{
    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
            return w;
        }
    }
}
