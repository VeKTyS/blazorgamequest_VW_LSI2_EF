namespace SharedModels.Models;

public class Monstre
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Health { get; set; } = 100;
    public int AttackPower { get; set; } = 10;
    public int Defense { get; set; } = 5;
    public int ScoreValue { get; set; } = 50;
    public Item droppedItem { get; set; } = new Item();
    public bool isBoss { get; set; } = false;
}