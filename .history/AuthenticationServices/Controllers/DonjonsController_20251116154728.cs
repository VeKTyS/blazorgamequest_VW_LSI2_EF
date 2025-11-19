using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;
using AuthenticationServices.Services;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonjonsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly DungeonGenerator _generator;

    public DonjonsController(ApplicationDbContext db, DungeonGenerator generator)
    {
        _db = db;
        _generator = generator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Donjons.Include(d => d.Salles).ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var d = await _db.Donjons.Include(x => x.Salles).FirstOrDefaultAsync(x => x.Id == id);
        return d == null ? NotFound() : Ok(d);
    }

    // POST api/donjons/generate?rooms=10&seed=123
    [HttpPost("generate")]
    public IActionResult Generate([FromQuery] int rooms = 10, [FromQuery] int? seed = null)
    {
        var donjon = _generator.Generate(Math.Max(1, rooms), seed);
        return CreatedAtAction(nameof(Get), new { id = donjon.Id }, donjon);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Donjon model)
    {
        _db.Donjons.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Donjon model)
    {
        if (id != model.Id) return BadRequest();
        var existing = await _db.Donjons.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Entry(existing).CurrentValues.SetValues(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var d = await _db.Donjons.FindAsync(id);
        if (d == null) return NotFound();
        _db.Donjons.Remove(d);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}