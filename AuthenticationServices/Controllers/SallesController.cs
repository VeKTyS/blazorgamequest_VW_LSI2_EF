using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SallesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public SallesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Salles.ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var s = await _db.Salles.FindAsync(id);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Salle model)
    {
        _db.Salles.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Salle model)
    {
        if (id != model.Id) return BadRequest();
        var existing = await _db.Salles.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Entry(existing).CurrentValues.SetValues(model);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var s = await _db.Salles.FindAsync(id);
        if (s == null) return NotFound();
        _db.Salles.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}