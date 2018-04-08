using Aniflix_WebAPI.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Models
{
    public class Anime
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string HomeUrl { get; set; }
        public string ImgUrl { get; set; }
        public bool Mature { get; set; }
        public ICollection<Episode> Episodes { get; set; }

        public Anime() {
            Episodes = new List<Episode>();
        }

        //public Anime(string title, string homeUrl, string imgUrl)
        //{
        //    Title = title;
        //    HomeUrl = homeUrl;
        //    ImgUrl = imgUrl;
        //    Episodes = new List<Episode>();
        //    Id = DataHelper.CreateMD5(Title);
        //}

        public Episode Add(Episode episode)
        {
            if (Episodes.First(e => e.Id == episode.Id) ==null)
            {
                Episodes.Add(episode);
                return episode;
            }
            return null;
        }
    }
}
