using System;

namespace SharedModels.Models
{
    public class AdventureResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PlayerId { get; set; }
        public DateTimeOffset PlayedAt { get; set; } = DateTimeOffset.UtcNow;
        public int Score { get; set; }
        public bool IsDead { get; set; }
        public IEnumerable<string> Events { get; set; } = Array.Empty<string>();
    }
}