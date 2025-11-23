using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class PlayerStateService : IPlayerStateService
{
    public Player? CurrentPlayer { get; private set; }

    public void SetCurrent(Player player)
    {
        CurrentPlayer = player;
    }

    public void Clear()
    {
        CurrentPlayer = null;
    }
}
