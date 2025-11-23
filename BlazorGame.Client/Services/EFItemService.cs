using BlazorGame.Client.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class EFItemService : IItemService
{
    private readonly IDbContextFactory<GameDbContext> _factory;
    private readonly Random _rnd = new();

    public EFItemService(IDbContextFactory<GameDbContext> factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<Item> GetAllItems()
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.Items.ToList().AsReadOnly();
    }

    public Item? GetById(Guid id)
    {
        using var ctx = _factory.CreateDbContext();
        return ctx.Items.Find(id);
    }

    public Item AddItem(Item item)
    {
        using var ctx = _factory.CreateDbContext();
        ctx.Items.Add(item);
        ctx.SaveChanges();
        return item;
    }

    public bool RemoveItem(Guid id)
    {
        using var ctx = _factory.CreateDbContext();
        var it = ctx.Items.Find(id);
        if (it == null) return false;
        ctx.Items.Remove(it);
        ctx.SaveChanges();
        return true;
    }

    public Item? TakeRandomItem()
    {
        using var ctx = _factory.CreateDbContext();
        var all = ctx.Items.ToList();
        if (!all.Any()) return null;
        var picked = all[_rnd.Next(all.Count)];
        // remove from store to simulate taking it
        ctx.Items.Remove(picked);
        ctx.SaveChanges();
        return picked;
    }
}
