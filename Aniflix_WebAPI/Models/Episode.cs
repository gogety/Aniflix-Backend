using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Models
{
    public class Episode
    {
        // TODO : handle id in db context
        public int Id { get; set; }
        public string Title { get; set; }
        public string Anime { get; set; }
        // TODO : this could be an IMG directly
        public string ImgURL { get; set; }
        // TODO : DetailsURL should not be saved here, as it depends on the connector
        public string DetailsURL { get; set; }
        public string VideoURL { get; set; }

        public Episode() { }

        public Episode(string title, string anime, string imgURL, string detailsURL)
        {
            Title = title;
            Anime = anime;
            ImgURL = imgURL;
            DetailsURL = detailsURL;
            Id = $"{Anime} {Title}".GetHashCode();
            VideoURL = string.Empty;
        }
    }
}
