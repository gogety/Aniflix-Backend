using Aniflix_WebAPI.Logic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aniflix_WebAPI.Models
{
    public class AniContext:DbContext
    {
     
        public AniContext(DbContextOptions<AniContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //define relationships using fluent API
            //modelBuilder.Entity<Anime>()
            //    .HasMany(a => a.Episodes);
            //modelBuilder.Entity<Anime>()
            //    .HasKey(d => DataHelper.CreateMD5(d.Title));

            modelBuilder.Entity<Anime>()
                .HasMany(a => a.Episodes)
                .WithOne(e => e.Anime);
            
            modelBuilder.Entity<Episode>()
                .HasMany(e => e.SourceRepoLinks);

            //modelBuilder.Entity<Episode>()
            //    .HasKey(e => DataHelper.CreateMD5($"{e.Anime.Title} {e.Title}"));
        }

        public DbSet<Anime> Animes { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<EpisodeSourceRepoLink> EpisodeSourceRepoLinks{ get; set; }

    }
}
