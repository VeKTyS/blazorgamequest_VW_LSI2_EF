using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class InMemoryPlayerService : IPlayerService
{
    private readonly List<Player> _players = new();

    public InMemoryPlayerService()
    {
        // seed with a demo admin and a player
        _players.Add(new Player { Username = "demo_player", Password = "pass", TotalScore = 0 });
        _players.Add(new Player { Username = "admin", Password = "admin" });
    }

    public Player CreatePlayer(string username, string password)
    {
        var p = new Player { Username = username, Password = password, TotalScore = 0 };
        _players.Add(p);
        return p;
    }

    public bool DeletePlayer(Guid id)
    {
        var p = _players.FirstOrDefault(x => x.Id == id);
        if (p == null) return false;
        _players.Remove(p);
        return true;
    }

    public IReadOnlyList<Player> GetAllPlayers() => _players.AsReadOnly();

    public Player? GetPlayerById(Guid id) => _players.FirstOrDefault(x => x.Id == id);

    public Player? Authenticate(string username, string password)
    {
        return _players.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && x.Password == password);
    }

    public void AddOrUpdate(Player player)
    {
        var existing = _players.FirstOrDefault(x => x.Id == player.Id);
        if (existing == null)
        {
            _players.Add(player);
            return;
        }
        existing.Username = player.Username;
        existing.Password = player.Password;
        existing.TotalScore = player.TotalScore;
    }
}
