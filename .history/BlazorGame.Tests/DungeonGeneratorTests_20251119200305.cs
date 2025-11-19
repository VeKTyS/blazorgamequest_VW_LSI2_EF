using Xunit;
using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
AuthenticationServices.Services
using SharedModels.Models;
using System.Linq;

public class DungeonGeneratorTests
{
    [Fact]
    public void Generate_CreatesRequestedNumberOfRooms_WhenEnoughTemplates()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "test_db_gen1")
            .Options;

        using var db = new ApplicationDbContext(options);
        // seed templates
        db.RoomTemplates.AddRange(Enumerable.Range(0, 15).Select(i =>
            new RoomTemplate { Name = $"T{i}", Description = "tpl", Weight = 1.0 }));
        db.SaveChanges();

        var gen = new DungeonGenerator(db);
        var donjon = gen.Generate(10, seed: 123);

        Assert.NotNull(donjon);
        Assert.Equal(10, donjon.Salles.Count);
    }
}