using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aniflix_WebAPI.Models;
using Aniflix_WebAPI.Logic.Connectors;

namespace Aniflix_WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class EpisodesController : Controller
    {
        private readonly AniContext _context;

        public EpisodesController(AniContext context)
        {
            _context = context;
        }

        // GET: api/Episodes - Get the latest/trending episodes 
        [HttpGet]
        public IEnumerable<Episode> GetEpisodes()
        {
            if(_context.Episodes.FirstOrDefault() == null)
            {
                // Get all episodes that are not already in the database
                
                foreach (Episode epi in AniWatcher.Connector.GetEpisodesList())
                {
                    if (_context.Episodes.Find(epi.Id) == null)
                    {
                        _context.Add(epi);
                        System.Diagnostics.Debug.WriteLine($"ADDED {epi.Id} {epi.AnimeTitle} {epi.Title}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"REFUSED {epi.Id} {epi.AnimeTitle} {epi.Title}");
                    }
                        
                }
                _context.SaveChanges();
            }

            return _context.Episodes;

        }

        // GET: api/Episodes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEpisode(string id, string repoLinkId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var episode = await _context.Episodes.Include(e => e.SourceRepoLinks).SingleAsync(m => m.Id == id);

            if (episode == null)
            {
                return NotFound();
            }
            //Boolean needsUpdate = String.IsNullOrEmpty(episode.VideoURL);

            if (episode.SourceRepoLinks.Count == 0)
            {
                AniWatcher.Connector.LoadEpisodeLinks(_context,episode);
                //_context.Update(episode);
                // Maybe we should not wait for the save confirmation ?
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrEmpty(repoLinkId))
                return Json(episode);

            string videoURL = AniWatcher.Connector.GetVideo(_context, episode, repoLinkId);

            return Json(videoURL);
        }

        // PUT: api/Episodes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEpisode([FromRoute] string id, [FromBody] Episode episode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != episode.Id)
            {
                return BadRequest();
            }

            _context.Entry(episode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EpisodeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Episodes
        [HttpPost]
        public async Task<IActionResult> PostEpisode([FromBody] Episode episode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEpisode", new { id = episode.Id }, episode);
        }

        // DELETE: api/Episodes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEpisode([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var episode = await _context.Episodes.SingleOrDefaultAsync(m => m.Id == id);
            if (episode == null)
            {
                return NotFound();
            }

            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();

            return Ok(episode);
        }

        private bool EpisodeExists(string id)
        {
            return _context.Episodes.Any(e => e.Id == id);
        }
    }
}