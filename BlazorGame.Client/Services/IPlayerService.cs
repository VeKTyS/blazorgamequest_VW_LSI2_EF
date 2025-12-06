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

    // Méthode pour enregistrer une partie
    Task EndAndSaveAsync(Guid playerId, int score, string details);

    // Récupérer l'historique des parties d'un joueur
    Task<IReadOnlyList<AdventureResult>> GetGameResultsForPlayerAsync(Guid playerId);

    // NOUVELLE MÉTHODE : Récupérer l'historique de toutes les parties de tous les joueurs
    Task<IReadOnlyList<AdventureResult>> GetAllGameResultsAsync();
}
