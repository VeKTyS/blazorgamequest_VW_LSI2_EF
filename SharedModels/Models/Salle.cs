namespace SharedModels.Models;

public class Salle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Item> Items { get; set; } = new();
    public List<Monstre> Monstres { get; set; } = new();
    public List<Salle> ConnectedRooms { get; set; } = new();
}