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
    [Route("api/Animes")]
    public class AnimesController : Controller
    {
        private readonly AniContext _context;

        public AnimesController(AniContext aniContext)
        {
            _context = aniContext;
        }

        // GET: api/Animes
        [HttpGet]
        public IEnumerable<Anime> GetAnimes(int? more = 0)
        {
            BaseConnector connector = AniWatcher.Connector;
            //List<Anime> list = connector.GetAnimesList(_aniContext);
            //  var animes = _aniContext.Animes.Include(a => a.Episodes);
            if (false) { 
                foreach (var a in _context.Animes)
                {
                    _context.Remove(a);
                }
                foreach (var e in _context.Episodes)
                {
                    _context.Remove(e);
                }
            }
            //foreach (Anime a in list)
            //{
            //    if (_aniContext.Animes.Find(a.Id) == null)
            //    {
            //        _aniContext.Animes.Add(a);
            //    }
            //    //foreach (Episode e in a.Episodes)
            //    //{
            //    //    if (_aniContext.Episodes.Find(e.Id) == null)
            //    //    {
            //    //        _aniContext.Episodes.Add(e);

            //    //    }
            //    //}
            //}

            //more logic needs to change. Currently, 0 => just load whatever is already in the db; 1 => load one more page; 2 => load many more pages
            if (_context.Animes.Count() == 0)
                more = 1;
            if (more >0)
            {
                connector.LoadAnimesList(_context, (more==2));
                _context.SaveChanges();
            }
           
            // we need to include the episodes, otherwise they are not loaded
            // see https://docs.microsoft.com/en-us/ef/core/querying/related-data
            return _context.Animes.Include(a => a.Episodes);
        }

        // GET: api/Animes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnime([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anime = await _context.Animes.SingleOrDefaultAsync(m => m.Id == id);

            if (anime == null)
            {
                return NotFound();
            }

            return Ok(anime);
        }

        // PUT: api/Animes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnime([FromRoute] string id, [FromBody] Anime anime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != anime.Id)
            {
                return BadRequest();
            }

            _context.Entry(anime).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnimeExists(id))
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

        // POST: api/Animes
        [HttpPost]
        public async Task<IActionResult> PostAnime([FromBody] Anime anime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Animes.Add(anime);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnime", new { id = anime.Id }, anime);
        }

        // DELETE: api/Animes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnime([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anime = await _context.Animes.SingleOrDefaultAsync(m => m.Id == id);
            if (anime == null)
            {
                return NotFound();
            }

            _context.Animes.Remove(anime);
            await _context.SaveChangesAsync();

            return Ok(anime);
        }

        private bool AnimeExists(string id)
        {
            return _context.Animes.Any(e => e.Id == id);
        }
    }
}