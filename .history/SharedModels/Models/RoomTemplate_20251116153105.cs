namespace SharedModels.Models;

public enum RoomType { Combat, Treasure, Trap, Fountain, Boss, Challenge, Encounter, Puzzle }

public class RoomTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Type générique de la salle (utile pour filtrer/contraindre la génération)
    public RoomType Type { get; set; } = RoomType.Encounter;

    // Poids pour sélection aléatoire pondérée
    public double Weight { get; set; } = 1.0;

    // Difficulty range pour adapter la difficulté selon la progression
    public int MinDifficulty { get; set; } = 1;
    public int MaxDifficulty { get; set; } = 3;

    // Pools référencant des ids de monstres/objets (optionnel)
    public List<Guid> PossibleMonstreIds { get; set; } = new();
    public List<Guid> PossibleItemIds { get; set; } = new();
}