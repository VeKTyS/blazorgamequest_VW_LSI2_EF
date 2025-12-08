using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace BlazorGame.Client.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<AdventureResult> AdventureResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration pour la relation avec Item si nécessaire
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Inventory)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
