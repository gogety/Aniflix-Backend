using Aniflix_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Logic
{
    public abstract class BaseConnector
    {
        protected string _baseURL;
        protected abstract Episode CreateEpisode(Object obj);
        protected string FormatHttp(string url)
        {
            //When sent with a //url format, assume is is https
            if (url.Length > 1 && url.Substring(0, 2) == "//")
            {
                url = $"https:{url}";
            }
            return url;
        }

        public abstract List<Episode> GetList();
        public abstract string GetVideo(Episode episode);

    }
}
