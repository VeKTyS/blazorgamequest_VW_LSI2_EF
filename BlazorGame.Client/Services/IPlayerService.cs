using SharedModels.Models;
namespace BlazorGame.Client.Services;

public interface IPlayerService
{
    IReadOnlyList<Player> GetAllPlayers();
    Player? GetPlayerById(Guid id);
    Player CreatePlayer(string username, string password);
    bool DeletePlayer(Guid id);
    Player? Authenticate(string username, string password);
    void AddOrUpdate(Player player);
}
