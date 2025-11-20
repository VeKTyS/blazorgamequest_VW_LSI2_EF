using SharedModels.Models;

namespace BlazorGame.Client.Services;

public interface IPlayerStateService
{
    Player? CurrentPlayer { get; }
    void SetCurrent(Player player);
    void Clear();
}
