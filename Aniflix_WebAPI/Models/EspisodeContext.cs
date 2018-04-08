using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Models
{
    public class EpisodeContext : DbContext
    {
        public EpisodeContext(DbContextOptions<AniContext> options): base (options) 
        {

        }

        public DbSet<Episode> Episodes  { get; set; }
    }
}
