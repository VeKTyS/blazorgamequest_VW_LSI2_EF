using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class DungeonGeneratorService : IDungeonGenerator
{
    private static readonly string[] RoomNames =
    {
        "Entrée", "Couloir", "Salle du Trésor", "Crypte", "Sanctuaire", "Antre", "Chambre du Boss"
    };

    private static readonly string[] RoomDescriptions =
    {
        "Des torches vacillantes éclairent faiblement les murs.",
        "Un couloir jonché de débris et de toiles d'araignée.",
        "Une salle remplie de coffres et d'artefacts brillants.",
        "D'anciens sarcophages tapissent les murs, une atmosphère sinistre.",
        "Un sanctuaire abandonné aux dieux anciens.",
        "Une pièce sombre où l'on sent une présence hostile.",
        "La salle finale, une ombre maléfique plane ici."
    };

    private readonly Random _rnd = new();

    public Donjon Generate(int roomsCount = 5, string? name = null)
    {
        var d = new Donjon { Name = name ?? $"Donjon #{_rnd.Next(1000, 9999)}" };

        // Create rooms
        for (var i = 0; i < roomsCount; i++)
        {
            var r = new Salle
            {
                Name = RoomNames[_rnd.Next(RoomNames.Length)] + (i == 0 ? " (Entrée)" : i == roomsCount - 1 ? " (Boss)" : ""),
                Description = RoomDescriptions[_rnd.Next(RoomDescriptions.Length)]
            };

            // Randomly add an item
            if (_rnd.NextDouble() < 0.6)
            {
                r.Items.Add(new Item
                {
                    Name = "Potion de vie",
                    Description = "Soigne légèrement",
                    HealthEffect = 10,
                    ScoreEffect = 5
                });
            }

            // Randomly add a monster
            if (_rnd.NextDouble() < 0.7 || i == roomsCount - 1)
            {
                r.Monstres.Add(new Monstre
                {
                    Name = i == roomsCount - 1 ? "Boss" : ("Monstre " + _rnd.Next(1, 99)),
                    Health = i == roomsCount - 1 ? 150 : 50 + _rnd.Next(0, 50),
                    AttackPower = 5 + _rnd.Next(0, 10),
                    Defense = 2 + _rnd.Next(0, 6),
                    ScoreValue = i == roomsCount - 1 ? 500 : 50 + _rnd.Next(0, 100),
                    isBoss = i == roomsCount - 1
                });
            }

            d.Salles.Add(r);
        }

        // Connect rooms as a linear suite (simple graph)
        for (var i = 0; i < d.Salles.Count; i++)
        {
            if (i > 0) d.Salles[i].ConnectedRooms.Add(d.Salles[i - 1]);
            if (i < d.Salles.Count - 1) d.Salles[i].ConnectedRooms.Add(d.Salles[i + 1]);
        }

        return d;
    }
}
