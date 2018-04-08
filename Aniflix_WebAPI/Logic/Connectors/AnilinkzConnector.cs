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
            string animeTitle = node.QuerySelector("a.ser").Attributes["Title"].Value;
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

            string detailsURL = node.QuerySelector("a.ep").Attributes["href"].Value;

            return new Episode
            {
                AnimeId = DataHelper.CreateMD5(animeTitle),
                Id = DataHelper.CreateMD5($"{animeTitle}{title}"),
                Title = title,
                DetailsURL = detailsURL
            };
        }

        protected override Anime CreateAnime(object obj)
        {
            HtmlNode node;
            if (!(obj is HtmlNode))
            {
                throw new InvalidCastException();
            }
            node = (HtmlNode)obj;

            string animeTitle = node.QuerySelector("a.ser").Attributes["Title"].Value;
            
            string imgURL = node.QuerySelector("span.img").Attributes["style"].Value;
            imgURL = imgURL.Substring(imgURL.IndexOf("(") + 1);
            imgURL = imgURL.Substring(0, imgURL.IndexOf(")"));
            imgURL = $"{_baseURL}{imgURL}";

            string homeURL = node.QuerySelector("a.ser").Attributes["href"].Value;

            return new Anime
            {
                Title = animeTitle,
                Id = DataHelper.CreateMD5(animeTitle),
                HomeUrl = homeURL,
                ImgUrl = imgURL,
                Mature = animeTitle.ToLower().Contains("Uncensored")
            };
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

        public override void LoadAnimesList(AniContext context)
        {
            if (context.Animes.Count() == 0)
            {
                getAndLoadTrendingAndLatestEpisodes(context);
            }
            else if(context.Animes.Count() <=35) 
            {
                getAndLoadMoreTrendingAndLatestEpisodes(context);
            }
            
            
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

        private void getAndLoadTrendingAndLatestEpisodes(AniContext context)
        {
            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc(_baseURL, xPathWaitFor: "//ul[@id='trendinglist']");
            extractAndLoadTrendingEpisodes(doc,context);
            extractAndLoadLatestEpisodes(doc, context);
           // getAndLoadLatestEpisodes(context, 2, 6);
        }

        private void getAndLoadMoreTrendingAndLatestEpisodes(AniContext context)
        {
            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc(_baseURL, xPathWaitFor: "//ul[@id='trendinglist']");
            //extractAndLoadTrendingEpisodes(doc, context);
            //extractAndLoadLatestEpisodes(doc, context);
            getAndLoadLatestEpisodes(context, 2, 6);
        }

        private void getAndLoadLatestEpisodes(AniContext context, int page = 1, int upToPage = 1)
        {
            if (page > upToPage)
            {
                return;
            }

            HtmlDocument doc = BrowserHelper.ExecuteWebRequestHTTPWithJsDoc($"{_baseURL}/?p={page}", "//ul[@id='latestlist']");
            extractAndLoadLatestEpisodes(doc, context);

            if (page == upToPage)
            {
                return;
            }

            getAndLoadLatestEpisodes(context, page + 1, upToPage);
        }

        private void extractAndLoadTrendingEpisodes(HtmlDocument doc, AniContext context)
        {
            Episode episode = null;
            Anime anime = null;
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//ul[@id='trendinglist']").Descendants("li"))
            {
                episode = CreateEpisode(node);
                anime = CreateAnime(node);
                if (_filterUncensored && !anime.Mature)
                {
                    loadEpisodeAndAnimeToContext(context, episode, anime);
                }
            }
        }

        private void extractAndLoadLatestEpisodes(HtmlDocument doc, AniContext context)
        {
            Anime anime = null;
            Episode episode = null;
            try
            {
                foreach (HtmlNode node in doc.DocumentNode.QuerySelectorAll("#latestlist > li"))
                {
                    episode = CreateEpisode(node);
                    anime = CreateAnime(node);

                    if (_filterUncensored && !anime.Mature)
                    {
                        loadEpisodeAndAnimeToContext(context, episode, anime);
                    }
                }
            }
            catch { }
        }

        private void loadEpisodeAndAnimeToContext(AniContext context, Episode episode, Anime anime)
        {
            if (context.Episodes.Find(episode.Id) == null)
            {
                context.Add(episode);
            }
            if (context.Animes.Find(anime.Id) == null)
            {
                context.Add(anime);
            }
        }

        #region unused 

        public override List<Episode> GetEpisodesList()
        {
            List<Episode> episodes = new List<Episode>();

            //using (BrowserManager browser = new BrowserManager(true))
            //{
            // episodes = getTrendingEpisodes(browser).Union(getLatestEpisodes(browser)).ToList<Episode>();
            episodes = getTrendingAndLatestEpisodes();

            //}

            return episodes;
        }

        public override List<Anime> GetAnimesList(AniContext context)
        {
            List<Anime> animes = new List<Anime>();
            // Anime ani = new Anime("Darling in the Franxx", "/series/darling-in-the-franxx", "/img/ser/fi/4215.jpg");
            Anime ani = new Anime
            {
                Title = "Darling in the Franxx",
                HomeUrl = "/series/darling-in-the-franxx",
                ImgUrl = $"{_baseURL}/img/ser/fi/4215.jpg",
                Id = DataHelper.CreateMD5("Darling in the Franxx")
            };
            context.Add(ani);
            //ani.Episodes.Add(new Episode("Episode 12", ani.Id, "darling-in-the-franxx-episode-12"));
            // ani.Episodes.Add(new Episode {
            Episode epi = new Episode
            {
                Title = "Episode 12",
                AnimeId = ani.Id,
                DetailsURL = "/darling-in-the-franxx-episode-12",
                Id = DataHelper.CreateMD5($"{ani.Title} Episode 12")
            };
            context.Add(epi);
            // animes.Add(ani);
            return animes;
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
                if (_filterUncensored && !epi.AnimeTitle.ToLower().Contains("uncensored"))
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
                    if (_filterUncensored && !epi.AnimeTitle.ToLower().Contains("uncensored"))
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
                    if (!epi.AnimeTitle.ToLower().Contains("uncensored"))
                    {
                        list.Add(epi);
                    }
                }
            }
            catch { }

            return list;
        }

        #endregion
    }
}
