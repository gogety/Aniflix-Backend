using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Models
{
    //This model represents the repo links for an episode for each source and the source direct address to be used in the video tag
    // the source link seems to have a certain validity (different per source?) and the lastupdate value helps identify whether we should go grab a new link
    //For example, for repo anilinkz and episode A:
    // sMango -> http://anilinkz.to/episodeA?src=2 = repolink
    // https://fruitopia.com/13h54u3594 = sourcelink

    public class EpisodeSourceRepoLink
    {
        public string Id { get; set; }
        // TODO : replace with link to source or sourceId
        public string SourceName { get; set; }
        public Episode Episode { get; set; }
        public string RepoLink { get; set; }
        public string SourceLink { get; set; }
        public DateTime LastUpdate { get; set; }
        // for now we have only one repository - yagni ?
        //public Repo repositoy { get; set; }
    }
}
