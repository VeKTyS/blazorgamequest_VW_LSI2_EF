using AuthenticationServices.Data;
using AuthenticationServices.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseInMemoryDatabase("Auth_InMemory"));

// register DungeonGenerator
builder.Services.AddScoped<DungeonGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed the database with initial data (extend SeedData to include RoomTemplates)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    void SeedData(ApplicationDbContext context)
    {
        if (!context.Players.Any())
        {
            context.Players.AddRange(new[]
            {
                new Player { Username = "demo_player", Password = "pass", TotalScore = 0, Health = 100, IsAdmin = false },
                new Player { Username = "admin", Password = "admin", TotalScore = 0, Health = 100, IsAdmin = true }
            });
        }

        if (!context.RoomTemplates.Any())
        {
            context.RoomTemplates.AddRange(new RoomTemplate[]
            {
                new RoomTemplate { Name = "Couloir des Ombres", Description = "Un couloir sinistre. Un ennemi peut surgir.", Type = RoomType.Combat, Weight = 2.0, MinDifficulty = 1, MaxDifficulty = 2 },
                new RoomTemplate { Name = "Salle du Trésor", Description = "Un coffre ancien.", Type = RoomType.Treasure, Weight = 1.2, MinDifficulty = 1, MaxDifficulty = 3 },
                new RoomTemplate { Name = "Piège à Dalle", Description = "Sol piégé.", Type = RoomType.Trap, Weight = 1.0, MinDifficulty = 1, MaxDifficulty = 3 },
                new RoomTemplate { Name = "Fontaine de Vie", Description = "Une fontaine curative.", Type = RoomType.Fountain, Weight = 0.8, MinDifficulty = 1, MaxDifficulty = 1 },
                new RoomTemplate { Name = "Salle de l'Enigme", Description = "Une énigme attend.", Type = RoomType.Puzzle, Weight = 0.6, MinDifficulty = 2, MaxDifficulty = 4 },
                new RoomTemplate { Name = "Antechambre du Boss", Description = "La présence du boss se fait sentir.", Type = RoomType.Boss, Weight = 0.3, MinDifficulty = 4, MaxDifficulty = 6 }
            });
        }

        // seed minimal monsters/items if absent (so templates can reference real ids later)
        if (!context.Monsters.Any())
        {
            context.Monsters.AddRange(new[]
            {
                new Monstre { Name = "Gobelin", Health = 30, AttackPower = 5, Defense = 1, ScoreValue = 20 },
                new Monstre { Name = "Orc", Health = 80, AttackPower = 12, Defense = 4, ScoreValue = 75 }
            });
        }

        if (!context.Items.Any())
        {
            context.Items.AddRange(new[]
            {
                new Item { Name = "Potion de Vie", Description = "Soin 20 HP", ScoreValue = 10 },
                new Item { Name = "Pièce d'Or", Description = "Trésor", Value = 50 }
            });
        }

        context.SaveChanges();
    }

    SeedData(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Run();