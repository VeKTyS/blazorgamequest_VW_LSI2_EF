namespace SharedModels.Models;


public class AdventureResult
{
    public Guid PlayerId { get; set; }
    public DateTimeOffset PlayedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Score { get; set; }
    public bool IsDead { get; set; }
    public IEnumerable<string> Events { get; set; } = Array.Empty<string>();
}