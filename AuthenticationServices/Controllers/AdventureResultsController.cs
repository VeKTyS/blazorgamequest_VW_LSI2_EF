using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdventureResultsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public AdventureResultsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.AdventureResults.ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var r = await _db.AdventureResults.FindAsync(id);
        return r == null ? NotFound() : Ok(r);
    }

    [Authorize(Policy = "PlayerOnly")]
    [HttpPost]
    public async Task<IActionResult> Create(AdventureResult model)
    {
        _db.AdventureResults.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }
}