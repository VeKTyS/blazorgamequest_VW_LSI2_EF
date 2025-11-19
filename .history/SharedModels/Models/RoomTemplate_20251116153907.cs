using System;
using System.Collections.Generic;

namespace SharedModels.Models
{
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

        // Plage de difficulté (permet d'adapter selon progression)
        public int MinDifficulty { get; set; } = 1;
        public int MaxDifficulty { get; set; } = 3;

        // Pools d'ids (optionnel) : référençant monstres/items à cloner lors de l'instanciation
        public List<Guid> PossibleMonsterIds { get; set; } = new();
        public List<Guid> PossibleItemIds { get; set; } = new();
    }
}