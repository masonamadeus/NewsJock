using System;
using System.Collections.Generic;
using System.Text;

namespace NewsBuddy
{
    [Serializable]
    public class APTopic
    {
        public string nextPageLink {get;set;}
        public string APIresponse { get; set; }
        public int topicID { get; set; }
        public string topicName { get; set; }

    }
}
