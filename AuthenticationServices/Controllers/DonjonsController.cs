using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonjonsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DonjonsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Donjons.Include(d => d.Salles).ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var d = await _db.Donjons.Include(x => x.Salles).FirstOrDefaultAsync(x => x.Id == id);
        return d == null ? NotFound() : Ok(d);
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