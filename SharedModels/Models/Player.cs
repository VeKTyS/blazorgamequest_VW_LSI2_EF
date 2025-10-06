namespace SharedModels.Models;


public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public int TotalScore { get; set; }
}