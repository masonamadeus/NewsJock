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
using System.Xml;

namespace NewsBuddy
{



    public class APingestor
    {
        public string apiKey { get; set; }
        private const string reqUrl = "https://api.ap.org/media/v/content/";
        private HttpClient client;
        public EntityTagHeaderValue previousETag { get; set; }
        public EntityTagHeaderValue nextETag { get; set; }
        public bool isAuthorized { get; set; }

        public List<APObject> apFeedItems = new List<APObject>();

        public APingestor()
        {
            apiKey = Settings.Default.APapiKey;
            // call up feed and start it for all entitlements
        }
        
        public List<APObject> GetItems()
        {
            return apFeedItems;
        }



        public XmlDocument GetItem(string itemID)
        {
            Trace.WriteLine("getting ap item");
            if (client == null)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(reqUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            
            HttpResponseMessage response = client.GetAsync(String.Format("{0}?apikey={1}&in_my_plan=true", itemID, apiKey)).Result;
            if (response.IsSuccessStatusCode)
            {
                string reply = response.Content.ReadAsStringAsync().Result;
                Trace.WriteLine(reply);
                XmlDocument story = DownloadStory(reply);
                return story == null ? null : story;
            }
            else
            {
                return null;
            }
            
        }

        private XmlDocument DownloadStory(string json)
        {
            dynamic js = JsonConvert.DeserializeObject(json);
            if (js.error != null)
            {
                Trace.WriteLine(js.error);
                return null;
            }

            var downloadUrl = js.data.item.renditions.nitf.href;
            if (downloadUrl == null)
            {
                Trace.WriteLine("Download URL was null");
                return null;
            }
            else
            {
                HttpResponseMessage response = client.GetAsync(downloadUrl.ToString() + "&apikey=" + apiKey).Result;
                if (response.IsSuccessStatusCode)
                {
                    string rawXml =
                    response.Content.ReadAsStringAsync().Result;
                    XmlDocument story = new XmlDocument();
                    story.LoadXml(rawXml);
                    return story;
                }
                else
                {
                    Trace.WriteLine("HttpRequest was unsuccessful - downloadStory()");
                    return null;
                }
            }

        }

        public void GetFeed()
        {
            Trace.WriteLine("Getting Feed");
            if (client == null)
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(reqUrl);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(client.BaseAddress + "feed?apikey=" + apiKey + "&in_my_plan=true&versions=latest&text_links=plain",UriKind.Absolute),
                Method = HttpMethod.Get
            };

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (nextETag != null)
            {
                request.Headers.IfNoneMatch.Add(nextETag);
            }

            Trace.WriteLine(request.Headers.ToString());

            HttpResponseMessage response = client.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                apFeedItems.Clear();
                isAuthorized = true;
                string reply = response.Content.ReadAsStringAsync().Result;
                ProcessFeed(reply);
                if (nextETag != null)
                {
                    previousETag = new EntityTagHeaderValue(nextETag.ToString());

                }
                nextETag = new EntityTagHeaderValue(response.Headers.ETag.ToString());

                if (Debugger.IsAttached)
                {
                    Trace.WriteLine("Next ETag is: " + nextETag.ToString());
                    Trace.WriteLine(response.Headers.ETag.ToString());
                    Trace.WriteLine(response.StatusCode.ToString());
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotModified)
            {
                Trace.WriteLine("Not Modified. Same ETag");
                return;
            }
            else
            {
                isAuthorized = false;
                
                Trace.WriteLine(response.RequestMessage.ToString());

                Trace.WriteLine(response.StatusCode.ToString());
            }
        }

        private void ProcessFeed(string body)
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

            for (int entry = 0; entry < itemsCount; entry++)
            {
                ProcessEntry(itemsBlock[entry], entry);
            }

            

        }

        private void ProcessEntry ( dynamic Item, int index)
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
                    if (string.Equals(assoc.type.ToString(), "text") )
                    {
                        if (assoc.headline == null || string.Equals(assoc.headline.ToString(),""))
                        {
                            // ignore it
                        }
                        else
                        {
                            apFeedItems.Add(new APObject(assoc)
                            {
                                headline = assoc.headline != null ? assoc.headline.ToString() : "",
                                uri = assoc.uri != null ? assoc.uri.ToString() : "",
                                altID = assoc.altids.itemid != null ? assoc.altids.itemid.ToString() : ""
                            });
                        }
                        
                    }
                        
                }
            }
            else 
            {
                if (item.headline == null || string.Equals(item.headline.ToString(),""))
                {
                    // ignore that sucker
                }
                else
                {
                    apFeedItems.Add(new APObject(Item.item)
                    {

                        headline = item.headline != null ? (string)item.headline : "",
                        uri = item.uri != null ? (string)item.uri : "",
                        altID = item.altids.itemid != null ? item.altids.itemid.ToString() : ""

                    });
                }
                
            }

            

        }


        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                Trace.WriteLine("Disposing feed client");
                client = null;
            }
        }

    }
}

