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
        public APStory story { get; set; }
        public bool hasStory { get; set; }
        private APingestor ingestor { get; set; }
        public List<APObject> associations { get; set; }

        public APObject(object obj, APingestor _ingestor)
        {
            this.item = obj;
            this.ingestor = _ingestor;
            hasStory = false;
        }

        public bool GetStory()
        {
            if (!hasStory)
            {
                Trace.WriteLine("Fetching Story for " + headline);
                XmlDocument check = ingestor.GetItem(altID);
                if (check != null)
                {
                    this.story = new APStory(check);
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
