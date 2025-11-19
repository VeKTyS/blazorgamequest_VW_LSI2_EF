using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace AuthenticationServices.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Monstre> Monsters { get; set; }
        public DbSet<Donjon> Donjons { get; set; }
        public DbSet<Salle> Salles { get; set; }
        public DbSet<AdventureResult> AdventureResults { get; set; }
        public DbSet<Salle> Salles { get; set; }
    }
}