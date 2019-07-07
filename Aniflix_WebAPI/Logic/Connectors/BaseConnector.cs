using Aniflix_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Logic.Connectors
{
    public abstract class BaseConnector
    {
        protected string _baseURL;
        protected abstract Episode CreateEpisodeFromListing(Object obj);
        protected abstract Anime CreateAnimeFromListing(Object obj);
        protected string FormatHttp(string url)
        {
            //When sent with a //url format, assume is is https
            if (url.Length > 1 && url.Substring(0, 2) == "//")
            {
                url = $"https:{url}";
            }
            return url;
        }

        public abstract List<Episode> GetEpisodesList();
        public abstract List<Anime> GetAnimesList(AniContext context);
        public abstract void LoadAnimesList(AniContext context, bool loadBulk);
        public abstract void LoadAnimeDetails(AniContext context, Anime anime, Object obj = null);
        public abstract Anime LoadAnimeFromHomeURL(AniContext context, string homeURL);
        public abstract Episode LoadEpisodeLinksFromDetailsURL(AniContext context, string detailsURL);
        public abstract void LoadEpisodeLinks(AniContext context, Episode episode, Object obj = null);
        public abstract string GetVideo(AniContext context, Episode episode, string repoLinkId);

    }
}
