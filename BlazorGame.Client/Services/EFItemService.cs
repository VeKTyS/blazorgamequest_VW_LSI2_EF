using BlazorGame.Client.Data;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class EFItemService : IItemService
{
    private readonly IDbContextFactory<GameDbContext> _factory;
    private readonly IHttpClientFactory _httpFactory;
    private readonly Random _rnd = new();

    public EFItemService(IDbContextFactory<GameDbContext> factory, IHttpClientFactory httpFactory)
    {
        _factory = factory;
        _httpFactory = httpFactory;
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
        // Prefer calling API to persist with auth if available
        try
        {
            var client = _httpFactory.CreateClient("api");
            var resp = client.PostAsJsonAsync("api/Items", item).GetAwaiter().GetResult();
            if (resp.IsSuccessStatusCode)
            {
                return item;
            }
        }
        catch
        {
            // fallback to local in-memory for offline/demo
        }
        using var ctx = _factory.CreateDbContext();
        ctx.Items.Add(item);
        ctx.SaveChanges();
        return item;
    }

    public bool RemoveItem(Guid id)
    {
        try
        {
            var client = _httpFactory.CreateClient("api");
            var resp = client.DeleteAsync($"api/Items/{id}").GetAwaiter().GetResult();
            if (resp.IsSuccessStatusCode) return true;
        }
        catch
        {
        }
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
