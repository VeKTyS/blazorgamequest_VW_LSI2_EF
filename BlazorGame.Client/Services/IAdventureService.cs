using SharedModels.Models;

namespace BlazorGame.Client.Services;

public interface IAdventureService
{
    void StartAdventure(Player player, int rooms = 5);
    Salle? CurrentRoom { get; }
    Player CurrentPlayer { get; }
    string LastEvent { get; }
    bool IsFinished { get; }
    void Explore();
    void Search();
    void Rest();
    void Attack();
    void Flee();
}
