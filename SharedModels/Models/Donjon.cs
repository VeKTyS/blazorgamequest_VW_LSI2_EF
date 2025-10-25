namespace SharedModels.Models;

public class Donjon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<Salle> Salles { get; set; } = new();
}