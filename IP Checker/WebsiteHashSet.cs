﻿using System.Collections.Generic;

namespace IP_Checker
{
    public class WebsiteHashSet : HashSet<string>
    {
        public WebsiteHashSet() : base()
        {
        }
        public new void Add(string websiteFieldText)
        {
            if (!base.Contains(websiteFieldText))
            {
                lock (this)
                {
                    base.Add(websiteFieldText);
                }
                IPMonitor.UpdateWebsitesAction(this);
            }
        }
        public new void Remove(string websiteFieldText)
        {
            if (base.Contains(websiteFieldText))
            {
                lock (this)
                {
                    base.Remove(websiteFieldText);
                }
                IPMonitor.UpdateWebsitesAction(this);
            }
        }
    }
}
