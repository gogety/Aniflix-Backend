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
        protected abstract Episode CreateEpisode(Object obj);
        protected abstract Anime CreateAnime(Object obj);
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
        public abstract void LoadAnimesList(AniContext context);
        public abstract string GetVideo(Episode episode);

    }
}
