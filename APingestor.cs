using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NewsBuddy
{
    class APingestor
    {

        // ---- Set these values for your environment
        static readonly string ApiKey = "apusnkr7cnj9qfjuuhpwii1spe";

        static readonly string TmpDataDir = "./mediaapi_csharp_output_tmp";  // Tmp dir for saving everything fetched

        static readonly string Action = "feed";     // "feed" or "search"
        static readonly string FetchFeedLink = "http://api.ap.org/media/v/content/feed?q=productid:31989";
        static readonly string FetchSearchLink = "http://api.ap.org/media/v/content/search?q=type:picture";
        static readonly bool Verbose = true;        // Turns on a little extra Console output
        static readonly bool ProcessFeed = true;    // Process responses
        static readonly bool FollowNext = true;     // Follow next_page (and feed_href) links
        static readonly bool ProcessRenditions = true;  // Download renditions (see embedded comments wrt pricing)
        static readonly bool ProcessAssociations = true;    // Download/follow associations, and their contents
        static readonly int MaxLoops = 2;           // Max iterations of page follows


        // ----------------------------------
        static readonly string UserAgent = "APMediaApi-DemoC-1.0";
        static long TotalBytes = 0;
        static long HttpErrorCount = 0;

        static string NextLink = null;

        public static void Main()
        {
            var fetchLink = (Action == "feed" ? FetchFeedLink : FetchSearchLink);
            var lastNextLInk = "nolink";
            var rqNumber = 0;

            if (ApiKey == null)
            {
                Console.WriteLine("Must set ApiKey value, review/customize other 'static readonly' values too.");
                return;
            }

            if (!Directory.Exists(TmpDataDir))
                Directory.CreateDirectory(TmpDataDir);

            for (; rqNumber < MaxLoops; rqNumber++)
            {
                var rqTimeLabel = DateTime.UtcNow.ToString("HHmmss.ffff");
                var rqFileData = string.Format("{0}/{1}-{2}-{3}_{4}_{5}_data.json",
                  TmpDataDir, rqTimeLabel, Action, ApiKey, 0, rqNumber);

                string respJsonStr = issueSearchOrFeedRequest(fetchLink);
                if (respJsonStr != null)
                {
                    File.WriteAllText(rqFileData, respJsonStr, Encoding.UTF8);

                    processFeedOrSearchResponse(respJsonStr);
                    if (NextLink != null)
                        fetchLink = NextLink;

                    if (FollowNext && NextLink != null)
                        if (NextLink != lastNextLInk)
                        {
                            Console.WriteLine("NextLink: {0}", NextLink);
                            lastNextLInk = NextLink;
                        }
                        else
                            Console.WriteLine("No nextlink change");
                }
            }
            Console.WriteLine("\nCompleted {0} {1} cycles. HttpErrors:{2} Kb:{3:F2}",
              rqNumber, Action, HttpErrorCount, TotalBytes / 1024);

            Console.WriteLine("<Enter> to END");
            Console.ReadLine();
        }

        static string processFeedOrSearchResponse(string feedBody)
        {
            dynamic js = JsonConvert.DeserializeObject(feedBody);

            var rqId = js.id;
            if (js.error != null)
            {
                Console.WriteLine("ERROR: Check feed/search request, received error: {}", js.error);
                return null;
            }

            NextLink = null;
            var dataBlock = js.data;
            if (dataBlock == null)
            {
                Console.WriteLine("processFeedOrSearchResponse -- Error. Response is missing 'data' element!");
                return null;
            }

            var pageType = "next_page";
            var next_page = dataBlock.next_page;
            var itemsBlock = dataBlock.items;
            var num_items = (itemsBlock != null && itemsBlock.Count != null) ? itemsBlock.Count : 0;
            if (next_page == null)
            {
                // NOTE: Never happens from /feed
                // in /search responses, when reaching the "end" a 'feed_href' link may be provided that lets you
                // transition to a /feed model, continuing to monitor for newly arriving content matching your initial /search
                next_page = dataBlock.feed_href;
                pageType = "feed_href";
            }
            Console.WriteLine("processEntireFeed - Response with id:{0} items:{1} {2}:{3}",
              rqId, num_items, pageType, next_page);

            if (ProcessFeed)
                processAllItems(itemsBlock);

            NextLink = next_page;

            if (pageType == "feed_href")
                Console.WriteLine("\nprocessEntireFeed - Reached end of /search results, feed_href provided to transition to /feed ...");
            else if (next_page == null)
                Console.WriteLine("\nprocessEntireFeed - WARNING: Body has no next_page or feed_href");

            return NextLink;
        }

        static void processAllItems(dynamic itemsBlock)
        {
            for (int localEntry = 0; localEntry < itemsBlock.Count; localEntry++)
                process1metaItem(itemsBlock[localEntry], localEntry);
        }

        static void process1metaItem(dynamic metaItem, int entryIndex)
        {
            var item = metaItem.item;
            string iid = item.altids != null ? (string)item.altids.itemid : "";
            string etag = item.altids != null ? (string)item.altids.etag : "";
            var version = (int)item.version;
            var type = (string)item.type;
            var headline = item.headline != null ? item.headline.ToString() : "";
            var associations_len = item.associations != null ? ((JContainer)item.associations).Count : 0;

            var renditions_len = item.renditions != null ? ((JContainer)item.renditions).Count : 0;
            var entryDir = String.Format("{0}/{1}-{2}_{3}", TmpDataDir, iid, version, type);
            if (!Directory.Exists(entryDir))
                Directory.CreateDirectory(entryDir);
            var entryFile = String.Format("{0}/metadata_{1}-{2}_{3}.json", entryDir, iid, version, type);
            var jsItem = JsonConvert.SerializeObject(metaItem, Formatting.Indented);
            File.WriteAllText(entryFile, jsItem, Encoding.UTF8);

            var feTag = iid + "." + version;
            Console.WriteLine("\nEntry[{0}] :: itemid:{1} type:{2} headline:\"{3}\" renditions:{4} associations:{5}",
              entryIndex, feTag,
              type, headline, renditions_len, associations_len);

            if (ProcessAssociations && item.associations != null)
            {
                foreach (dynamic ac in item.associations)  // item.associations is an Object containing Objects, each with a name ressembling index values, "1", "2"
                {
                    var assocName = (string)ac.Name;
                    dynamic assoc = ac.Value;
                    Console.WriteLine("\tassociations.{0} type:{1} uri:{2}", assocName, (string)assoc.type, (string)assoc.uri);
                    issueItemRequest(entryDir, (string)assoc.uri, assoc);
                }
            }

            if (item.renditions != null)
            {
                foreach (dynamic rd in item.renditions)  // item.renditions is an Object containing Objects, each named for it's rendition type/format/etc...
                {
                    string rendName = (string)rd.Name;
                    dynamic rend = rd.Value;
                    string rendHref = (string)rend.href;
                    if (rend.priced != null)
                    {
                        var rendPricetag = (string)rend.pricetag;
                        // WARNING: Uncomment following line to supply price-acknowlegement on priced/charged downloads
                        // Not acknowledging priced ones will result in a download error (e.g. "402 - Payment Required") when following the link
                        // rendHref += "&pricetag=" + rendPricetag;
                        if (ProcessRenditions)
                            Console.WriteLine("\t(WARNING) renditions.{0} price:true must ACKNOWLEDGE by appending '&pricetag={1}'",
                              rendName, rendPricetag);
                    }
                    var rendExt = (string)rend.fileextension;
                    feTag = string.Format("{0}_{1}.{2}", rendName, (string)rend.contentid, rendExt);
                    var allV = rend.version_links;    // If you use /feed?versions=all you should be prepared to iterate on this array of anpa links
                    if (Verbose && !ProcessRenditions)
                        Console.WriteLine("\trenditions.{0}\t href:{1}", rendName, rendHref);
                    if (ProcessRenditions)
                        issueDownloadRequest(entryDir, rendHref, rendName, rendExt, feTag);
                }
            }
        }

        static void issueDownloadRequest(string toDir, string forLink, string ofType, string withExt, string forEntry = "standalone")
        {
            var oneLink = forLink;
            if (forLink.IndexOf("apikey=") < 0)
                oneLink += "&apikey=" + ApiKey;
            var rqTimeLabel = DateTime.UtcNow.ToString("HHmmss.ffff");
            Console.WriteLine("issueDownloadRequest: {0} to:{0} from:{1}", rqTimeLabel, toDir, forLink);
            var dldFileData = string.Format("{0}/{1}", toDir, forEntry);

            var webReq = WebRequest.CreateHttp(oneLink);
            webReq.UserAgent = UserAgent;
            webReq.AllowAutoRedirect = true;      // Important for rendition downloads - must allow redirects or follow them yourself
            try
            {
                var webResp = (HttpWebResponse)webReq.GetResponse();
                if (webResp.StatusCode != HttpStatusCode.OK)
                {
                    HttpErrorCount++;
                    Console.WriteLine("    .. HTTP-Error: {0} from download link:{1}", webResp.StatusCode, oneLink);
                }
                using (var respStream = webResp.GetResponseStream())
                using (var rawFile = File.Create(dldFileData))
                {
                    respStream.CopyTo(rawFile);
                    respStream.Flush();
                }
                Console.WriteLine(" rendition.{0} DOWNLOAD status:{1} rUri:{2}",
                  ofType, webResp.StatusCode, webResp.ResponseUri);
                Console.WriteLine("    .. saved-as:{0} bytes:{1}", dldFileData, webResp.ContentLength);
                if (webResp.ContentLength > 0)
                    TotalBytes += webResp.ContentLength;

            }
            catch (WebException wex)
            {
                Console.WriteLine("Received HTTP error from {0} error: {1}", oneLink, wex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Received error from {0} error: {1}", oneLink, ex.Message);
                return;
            }
        }

        static string issueSearchOrFeedRequest(string forLink)
        {
            string oneLink;
            if (forLink.IndexOf("apikey=") > -1)
                oneLink = forLink;
            else if (forLink.IndexOf("?") > 0)
                oneLink = forLink + "&apikey=" + ApiKey;
            else
                oneLink = forLink + "?apikey=" + ApiKey;

            var webReq = WebRequest.CreateHttp(oneLink);
            webReq.AllowAutoRedirect = true;
            webReq.UserAgent = UserAgent;
            try
            {
                HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
                using (var sr = new StreamReader(webResp.GetResponseStream(), Encoding.UTF8))
                    return sr.ReadToEnd();
            }
            catch (WebException wex)
            {
                Console.WriteLine("issueSearchOrFeedRequest. Received HTTP error from {0} error: {1}", oneLink, wex.Message);
                HttpErrorCount++;
                // TODO: Read the actual error (probably JSON)
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("issueSearchOrFeedRequest. Received error from {0} error: {1}", oneLink, ex.Message);
                HttpErrorCount++;
                return null;
            }
        }

        static void issueItemRequest(string parentDir, string assocUri, dynamic assoc)
        {
            string oneLink;
            if (assocUri.IndexOf("apikey=") > -1)
                oneLink = assocUri;
            else if (assocUri.IndexOf("?") > 0)
                oneLink = assocUri + "&apikey=" + ApiKey;
            else
                oneLink = assocUri + "?apikey=" + ApiKey;

            var assocIid = (assoc.altids != null && assoc.altids.itemid != null) ? (string)assoc.altids.itemid : "0";
            var assocType = (string)assoc.type;
            var assocDir = string.Format("{0}/association_{1}_{2}", parentDir, assocType, assocIid);
            var assocEtag = (assoc.altids != null && assoc.altids.etag != null) ? (string)assoc.altids.etag : "0";
            Console.WriteLine(" -- Association:{0} etag:{1}", assocIid, assocEtag);

            var webReq = WebRequest.CreateHttp(oneLink);
            webReq.AllowAutoRedirect = true;
            webReq.UserAgent = UserAgent;
            try
            {
                HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
                if (webResp.StatusCode != HttpStatusCode.OK)
                {
                    HttpErrorCount++;
                    Console.WriteLine("    .. HTTP-Error: {0} from metadata /content/ link:{1}", webResp.StatusCode, oneLink);
                }
                using (var sr = new StreamReader(webResp.GetResponseStream(), Encoding.UTF8))
                {
                    var itemBody = sr.ReadToEnd();
                    dynamic js = JsonConvert.DeserializeObject(itemBody);
                    dynamic jsData = js.data;
                    process1metaItem(jsData, 0);
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine("issueItemRequest -- Received HTTP error from {0} error: {1}", oneLink, wex.Message);
                // TODO: Read the actual error (probably JSON)
                HttpErrorCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("issueItemRequest -- Received error from {0} error: {1}", oneLink, ex.Message);
                HttpErrorCount++;
            }

        }

    }
}

