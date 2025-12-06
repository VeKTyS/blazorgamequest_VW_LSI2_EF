using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlayersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<Player>> PostPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        }

        // PUT: api/Players/{id}/score
        [HttpPut("{id:guid}/score")]
        public async Task<IActionResult> PutScore(Guid id, [FromBody] UpdateScoreDto dto)
        {
            if (dto == null) return BadRequest();

            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            player.TotalScore = dto.Score;
            _context.Players.Update(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id:guid}/scores")]
        public async Task<ActionResult<IEnumerable<AdventureResult>>> GetPlayerScores(Guid id)
        {
            var results = await _context.AdventureResults
                .Where(r => r.PlayerId == id)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            if (!results.Any())
            {
                return NotFound(); // Aucun score trouvé pour ce joueur
            }

            return Ok(results);
        }
    }

    // simple DTO
    public class UpdateScoreDto
    {
        public int Score { get; set; }
    }
}