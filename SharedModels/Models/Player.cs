namespace SharedModels.Models;


public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int Health { get; set; } = 100;
    public bool IsAdmin { get; set; } = false;
    public List<Item> Inventory { get; set; } = new();
}