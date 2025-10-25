using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using BlazorGame.Client.Data;

namespace BlazorGame.Client.Services;

public class EFPlayerService : IPlayerService
{
    private readonly IDbContextFactory<GameDbContext> _contextFactory;

    public EFPlayerService(IDbContextFactory<GameDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IReadOnlyList<Player> GetAllPlayers()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.Include(p => p.Inventory).ToList().AsReadOnly();
    }

    public Player? GetPlayerById(Guid id)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players.Include(p => p.Inventory).FirstOrDefault(p => p.Id == id);
    }

    public Player CreatePlayer(string username, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        var player = new Player 
        { 
            Username = username, 
            Password = password, 
            TotalScore = 0, 
            Health = 100, 
            IsAdmin = false 
        };
        context.Players.Add(player);
        context.SaveChanges();
        return player;
    }

    public bool DeletePlayer(Guid id)
    {
        using var context = _contextFactory.CreateDbContext();
        var player = context.Players.Find(id);
        if (player == null) return false;
        
        context.Players.Remove(player);
        context.SaveChanges();
        return true;
    }

    public Player? Authenticate(string username, string password)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Players
            .Include(p => p.Inventory)
            .FirstOrDefault(p => p.Username.ToLower() == username.ToLower() && p.Password == password);
    }

    public void AddOrUpdate(Player player)
    {
        using var context = _contextFactory.CreateDbContext();
        var existing = context.Players.Find(player.Id);
        
        if (existing == null)
        {
            context.Players.Add(player);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(player);
            existing.Inventory = player.Inventory;
        }
        
        context.SaveChanges();
    }
}
