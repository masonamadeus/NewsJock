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
        private bool m_hasStory { get; set; }
        private APingestor ingestor { get; set; }
        public List<APObject> associations { get; set; }
        public bool isAssocParent { get; private set; }

        public APObject(object obj, APingestor _ingestor, bool isParent = false)
        {
            this.item = obj;
            this.ingestor = _ingestor;
            if (isParent)
            {
                this.associations = new List<APObject>();
                this.isAssocParent = true;
            }
            m_hasStory = false;
        }

        public bool HasStory
        {
            get
            {
                if (isAssocParent)
                {
                    return false;
                }
                if (!m_hasStory)
                {
                    Trace.WriteLine("Fetching Story for " + headline);
                    XmlDocument check = ingestor.GetItem(altID);
                    if (check != null)
                    {
                        this.story = new APStory(check);
                        m_hasStory = true;
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

            private set { }
        }



    }
}
