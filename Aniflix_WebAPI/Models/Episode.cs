using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aniflix_WebAPI.Logic;

namespace Aniflix_WebAPI.Models
{
    public class Episode
    {
        // TODO : handle id in db context
        public string Id { get; set; }
        public string Title { get; set; }
        public string FullTitle { get; set; }
        public string AnimeTitle { get; set; }
        
        //I don't know why we need both Anime and AnimeId ?
        public string AnimeId { get; set; }
        public Anime Anime { get; set; }
        
        // TODO : DetailsURL should not be saved here, as it depends on the connector
        public string DetailsURL { get; set; }

        public ICollection<EpisodeSourceRepoLink> SourceRepoLinks { get; set; }

        // TODO : this should go in a separate table?, as we may have multiple links (by repo and by source)
        public string VideoURL { get; set; }

        public Episode() {
            VideoURL = string.Empty;
            SourceRepoLinks = new List<EpisodeSourceRepoLink>();
        }

        public Episode(string title, string animeId, string detailsURL)
        {
            Title = title;
            AnimeId = animeId;
            DetailsURL = detailsURL;
            Id = DataHelper.CreateMD5($"{AnimeTitle} {Title}");
            VideoURL = string.Empty;
        }
    }
}
