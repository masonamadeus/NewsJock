using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace NewsBuddy
{
    public class APObject
    {
        public string headline { get; set; }
        public string altID { get; set; }
        public string uri { get; set; }
        public object item { get; set; }
        public XmlDocument story { get; set; }
        public bool hasStory { get; set; }

        public APObject(object obj)
        {
            this.item = obj;
            hasStory = false;
        }

        public bool GetStory(APingestor ingestor)
        {
            if (!hasStory)
            {
                Trace.WriteLine("Fetching Story for " + headline);
                this.story = ingestor.GetItem(altID);
                if (story != null)
                {
                    hasStory = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
            
        }

    }
}
