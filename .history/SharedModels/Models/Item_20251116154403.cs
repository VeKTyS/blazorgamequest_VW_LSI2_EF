namespace SharedModels.Models;

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HealthEffect { get; set; } // Positif pour soigner, négatif pour blesser
    public int ScoreValues { get; set; } // Points de score donnés
}
