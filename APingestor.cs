using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace NewsBuddy
{
    public class APObject
    {
        public string headline { get; set; }
        public string uri { get; set; }
        public object item { get; set; }

        public APObject(object obj)
        {
            this.item = obj;
        }
    }


    public class APingestor
    {
        public string apiKey = "apusnkr7cnj9qfjuuhpwii1spe";
        private const string feedReqUrl = "https://api.ap.org/media/v/content/feed";

        public List<APObject> apFeedItems = new List<APObject>();
        public APingestor()
        {
            GetFeed();
            // call up feed and start it for all entitlements
        }
        public APingestor(string key, string search)
        {
            this.apiKey = @"apikey=" + key;
            // call up feed and start it for search term. "feed?q=search&apikey={apikey}"
        }

        public List<APObject> GetItems()
        {
            return apFeedItems;
        }

        public void GetFeed()
        {

            apFeedItems.Clear();


            Trace.WriteLine("Getting Feed");
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(feedReqUrl);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("feed?apikey="+apiKey+"&in_my_plan=true").Result;
            if (response.IsSuccessStatusCode)
            {
                string reply = response.Content.ReadAsStringAsync().Result;
                ProcessFeed(reply);
            }
            else
            {
                Trace.WriteLine(response.StatusCode.ToString());
                Trace.WriteLine(response.RequestMessage.ToString());
            }
        }

        void ProcessFeed(string body)
        {
            dynamic js = JsonConvert.DeserializeObject(body);

            if (js.error != null)
            {
                Trace.WriteLine(js.error);
                return;
            }

            var dataBlock = js.data;
            if (dataBlock == null)
            {
                Trace.WriteLine("datablock was null on feed processing");
            }

            var itemsBlock = dataBlock.items;
            var itemsCount = (itemsBlock != null && itemsBlock.Count != null) ? itemsBlock.Count : 0;

            for (int entry = 0; entry < itemsBlock.Count; entry++)
            {
                ProcessEntry(itemsBlock[entry], entry);
            }

            foreach (APObject a in apFeedItems)
            {
                Trace.WriteLine(a.headline);
                Trace.WriteLine("________NEXT_ITEM________");
            }

        }

        void ProcessEntry ( dynamic Item, int index)
        {
            /*
            
            string iid = item.altids != null ? (string)item.altids.itemid : "";
            string etag = item.altids != null ? (string)item.altids.etag : "";
            string uri = item.uri != null ? (string)item.uri : "";
            var version = (int)item.version;
            var type = (string)item.type;
            var headline = item.headline != null ? item.headline.ToString() : "";
            */

            var item = Item.item;
            var associations_len = item.associations != null ? ((JContainer)item.associations).Count : 0;

            if (associations_len != 0)
            {
                foreach (dynamic ac in item.associations)
                {
                    dynamic assoc = ac.Value;
                    apFeedItems.Add(new APObject(assoc)
                    {
                        headline = assoc.headline != null ? assoc.headline.ToString() : "",
                        uri = assoc.uri != null ? assoc.uri.ToString() : ""
                    });
                }
            }
            else 
            {
                apFeedItems.Add(new APObject(Item.item)
                {

                    headline = item.headline != null ? (string)item.headline : "",
                    uri = item.uri != null ? (string)item.uri : ""

                });
            }

            

        }

    }
}

