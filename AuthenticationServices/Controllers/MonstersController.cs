using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonstersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public MonstersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Monsters.ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var m = await _db.Monsters.FindAsync(id);
        return m == null ? NotFound() : Ok(m);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Monstre model)
    {
        _db.Monsters.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Monstre model)
    {
        if (id != model.Id) return BadRequest();
        var existing = await _db.Monsters.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Entry(existing).CurrentValues.SetValues(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var m = await _db.Monsters.FindAsync(id);
        if (m == null) return NotFound();
        _db.Monsters.Remove(m);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}