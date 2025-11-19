// ...existing code...
void SeedData(ApplicationDbContext context)
{
    if (!context.Players.Any())
    {
        // ...existing player seed...
    }

    // seed monsters
    if (!context.Monsters.Any())
    {
        context.Monsters.AddRange(
            new Monstre { Name = "Gobelin", Health = 30, AttackPower = 5, Defense = 1, ScoreValue = 20 },
            new Monstre { Name = "Orc", Health = 80, AttackPower = 12, Defense = 4, ScoreValue = 75 }
        );
        context.SaveChanges();
    }

    // seed rooms (salles) avec rencontre
    if (!context.Salles.Any())
    {
        var gob = context.Monsters.First(m => m.Name == "Gobelin");
        var orc = context.Monsters.First(m => m.Name == "Orc");

        var salle1 = new Salle
        {
            Name = "Couloir des Ombres",
            Description = "Un couloir sinistre, un gobelin vous guette.",
            Type = RoomType.MonsterEncounter,
            Encounter = gob
        };

        var salle2 = new Salle
        {
            Name = "Salle du Trésor",
            Description = "Des coffres brillants !",
            Type = RoomType.Treasure,
            Items = new List<Item> { new Item { Name = "Potion de Vie", HealthEffect = 20, ScoreEffect = 0 } }
        };

        context.Salles.AddRange(salle1, salle2);
        context.SaveChanges();
    }
}namespace SharedModels.Models;

public class Salle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Item> Items { get; set; } = new();
    public List<Monstre> Monstres { get; set; } = new();
    public List<Salle> ConnectedRooms { get; set; } = new();
}