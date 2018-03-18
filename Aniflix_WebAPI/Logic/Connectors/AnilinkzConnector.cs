using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aniflix_WebAPI.Models;
using HtmlAgilityPack.CssSelectors.NetCore;
using HtmlAgilityPack;
using SeleniumBrowserStdLib;

namespace Aniflix_WebAPI.Logic.Connectors
{
    public class AnilinkzConnector : BaseConnector
    {
        private static BaseConnector connector;

        private bool _filterUncensored;

        private string _baseURLMobile;

        private AnilinkzConnector()
        {
            _baseURL = "http://anilinkz.to";
            _baseURLMobile = "http://m.anilinkz.to";
            _filterUncensored = true;
        }

        public static BaseConnector Connector {
            get
            {
                if (connector == null)
                    connector = new AnilinkzConnector();
                return connector;
            }
        }

        protected override Episode CreateEpisode(object obj)
        {
            HtmlNode node;
            if (!(obj is HtmlNode))
            {
                throw new InvalidCastException();
            }
            node = (HtmlNode)obj;

            string anime = node.QuerySelector("a.ser").Attributes["Title"].Value;
            string title = node.QuerySelector("span.title").InnerText;
            string added = string.Empty;
            try
            {
                added = node.QuerySelector("small").InnerText;
            }
            catch
            {
                added = "Today";
            }
            string imgURL = node.QuerySelector("span.img").Attributes["style"].Value;
            imgURL = imgURL.Substring(imgURL.IndexOf("(") + 1);
            imgURL = imgURL.Substring(0, imgURL.IndexOf(")"));
            imgURL = $"{_baseURL}{imgURL}";

            string detailsURL = node.QuerySelector("a.ep").Attributes["href"].Value;

            return new Episode (title, anime, imgURL, detailsURL);
        }

        public override string GetVideo(Episode episode)
        {
            if (!String.IsNullOrEmpty(episode.VideoURL))
                return episode.VideoURL;
            //using (BrowserManager browser = new BrowserManager(true))
            //{
                string sMangoAnilinkzLink = getSMangoAnilinkzLink(episode.DetailsURL);
                // string sMangoSourceLink = getSMangoSourceLink(sMangoAnilinkzLink);
                //string sMangoVideoLink = getSMangoVideoLink(sMangoSourceLink);
                string sMangoSourceLink = BrowserHelper.ExecuteWebRequestHTTPWithJs(FormatHttp(sMangoAnilinkzLink), xPathFilter : "//*[@class='spart']/iframe", attribute:"src", timeout: 10);
                string sMangoVideoLink = BrowserHelper.ExecuteWebRequestHTTPWithJs(FormatHttp(sMangoSourceLink), xPathFilter: "//*[@id='mgvideo_html5_api']", attribute: "src", timeout : 10 );
                // TODO : error handling
                if (!sMangoVideoLink.Contains("ERROR"))
                    episode.VideoURL = FormatHttp(sMangoVideoLink);
            //}
            return episode.VideoURL;

        }

        public override List<Episode> GetList()
        {

            List<Episode> episodes = new List<Episode>();

            //using (BrowserManager browser = new BrowserManager(true))
            //{
                // episodes = getTrendingEpisodes(browser).Union(getLatestEpisodes(browser)).ToList<Episode>();
                episodes = getTrendingAndLatestEpisodes();

            //}

            return episodes;
        }

        private string getSMangoAnilinkzLink(string urlSuffix)
        {
            List<HtmlNode> SMParams = new List<HtmlNode>();

            //HtmlDocument anilinkzDoc = executeWebRequestHTTP($"{_baseURLMobile}{urlSuffix}").Result;
            HtmlDocument anilinkzDoc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc($"{_baseURLMobile}{urlSuffix}", xPathFilter: "//div[@id='sources']");
            HtmlNode sources = anilinkzDoc.DocumentNode.SelectSingleNode("//div[@id='sources']");
            SMParams = sources.Descendants("a").ToList<HtmlNode>();
            //the first sMango link is usually better quality but also slower...
            //HtmlNode sMangoFirstSource = SMParams.FirstOrDefault(e => e.InnerText == "sMango");
            SMParams = SMParams.Where(e => e.InnerText == "sMango").ToList();
            HtmlNode sMangoFirstSource = SMParams.Count>1 ? SMParams[1]:SMParams[0];

            return sMangoFirstSource.Attributes["href"].Value;
        }

        private List<Episode> getTrendingAndLatestEpisodes()
        {
            //TODO : add to db instead of loading all the time (default SQLite, inMemory, PostGreSQL? ...)
            //Content is enough to fill a light model, not the full one with the actual video link

            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc(_baseURL, xPathWaitFor: "//ul[@id='trendinglist']");

            //extract trending and first page of latest from the home page, then the other pages 
            List<Episode> trending = extractTrendingEpisodes(doc);
            List<Episode> latestP1 = extractLatestEpisodes(doc);
            List<Episode> latestRest = getLatestEpisodes(page: 2, upToPage: 5);
            return extractTrendingEpisodes(doc).Union(latestP1).Union(latestRest).ToList();
        }

        private List<Episode> extractTrendingEpisodes(HtmlDocument doc)
        {
            //TODO : add to db instead of loading all the time (default SQLite, inMemory, PostGreSQL? ...)
            //Content is enough to fill a light model, not the full one with the actual video link

            List<Episode> list = new List<Episode>();
            Episode epi = null;
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//ul[@id='trendinglist']").Descendants("li"))
            {
                epi = CreateEpisode(node);
                if (_filterUncensored && !epi.Anime.ToLower().Contains("uncensored"))
                {
                    list.Add(epi);
                }
            }

            return list;
        }

        private List<Episode> extractLatestEpisodes(HtmlDocument doc)
        {
            //TODO : add to db instead of loading all the time (default SQLite, inMemory, PostGreSQL? ...)
            //Content is enough to fill a light model, not the full one with the actual video link

            List<Episode> list = new List<Episode>();
            //need one main tag to be parseable in xml
            // string webContent = "<root>" + executeWebrequest("fetch.php", parameters) + "</root>"; 

            Episode epi = null;
            // XDocument doc = XDocument.Parse(webContent);
            try
            {
                foreach (HtmlNode node in doc.DocumentNode.QuerySelectorAll("#latestlist > li"))
                {
                    epi = CreateEpisode(node);
                    if (_filterUncensored && !epi.Anime.ToLower().Contains("uncensored"))
                    {
                        list.Add(epi);
                    }
                }
            }
            catch { }

            return list;
        }

        private List<Episode> getTrendingEpisodes()
        {

            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc(_baseURL, "//ul[@id='trendinglist']");

            return extractTrendingEpisodes(doc);
        }

        private List<Episode> getLatestEpisodes(int page = 1, int upToPage=1)
        {
            if (page > upToPage)
                return new List<Episode>();

            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc($"{_baseURL}/?p={page}", "//ul[@id='latestlist']");

            if (page == upToPage)
                return extractLatestEpisodes(doc);

            return extractLatestEpisodes(doc).Union(getLatestEpisodes(page + 1, upToPage)).ToList();
        }

        private List<Episode> getLatestEpisodesPhp(int page = 1)
        {
            //TODO : add to db instead of loading all the time (default SQLite, inMemory, PostGreSQL? ...)
            //Content is enough to fill a light model, not the full one with the actual video link

            List<Episode> list = new List<Episode>();
            string parameters = $"?action=ongoing&page={page}&type=0";
            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc($"{_baseURL}/fetch.php{parameters}", "//li");
            //need one main tag to be parseable in xml
            // string webContent = "<root>" + executeWebrequest("fetch.php", parameters) + "</root>"; 

            Episode epi = null;
            // XDocument doc = XDocument.Parse(webContent);
            try
            {
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li"))
                {
                    epi = CreateEpisode(node);
                    if (!epi.Anime.ToLower().Contains("uncensored"))
                    {
                        list.Add(epi);
                    }
                }
            }
            catch { }

            return list;
        }
    }
}
