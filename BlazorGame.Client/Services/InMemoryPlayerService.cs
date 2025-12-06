using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class InMemoryPlayerService : IPlayerService
{
    private readonly List<Player> _players = new();
    private readonly List<AdventureResult> _results = new();
    private readonly IJSRuntime _js;

    private const string StorageKey = "blazorgame_adventure_results_all";

    public InMemoryPlayerService(IJSRuntime js)
    {
        _js = js;
        // seed example players
        var demo = new Player { Username = "demo_player", Password = "pass", TotalScore = 0, Health = 100, IsAdmin = false };
        var admin = new Player { Username = "admin", Password = "admin", TotalScore = 0, Health = 100, IsAdmin = true };
        _players.Add(demo);
        _players.Add(admin);
    }

    public Player CreatePlayer(string username, string password)
    {
        var p = new Player { Username = username, Password = password, TotalScore = 0, Health = 100, IsAdmin = false };
        _players.Add(p);
        return p;
    }

    public bool DeletePlayer(Guid id)
    {
        var p = _players.FirstOrDefault(x => x.Id == id);
        if (p == null) return false;
        _players.Remove(p);
        _results.RemoveAll(r => r.PlayerId == id);
        _ = PersistAllAsync(); // fire-and-forget persist
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
        if (existing == null) { _players.Add(player); return; }
        existing.Username = player.Username;
        existing.Password = player.Password;
        existing.TotalScore = player.TotalScore;
        existing.Health = player.Health;
        existing.IsAdmin = player.IsAdmin;
        existing.Inventory = player.Inventory;
    }


    // return history for one player (loads from storage if needed)
    public async Task<IReadOnlyList<AdventureResult>> GetGameResultsForPlayerAsync(Guid playerId)
    {
        if (!_results.Any())
        {
            await LoadAllAsync(); // S'assure que les données sont chargées depuis le localStorage
        }
        
        var results = _results
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.Date)
            .ToList();

        return await Task.FromResult(results);
    }

    // persist entire results list to localStorage
    private async Task PersistAllAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_results);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch
        {
            // ignore local persist errors (optional: log)
        }
    }

    // load entire results list from localStorage
    private async Task LoadAllAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var list = JsonSerializer.Deserialize<List<AdventureResult>>(json);
                if (list != null)
                {
                    _results.Clear();
                    _results.AddRange(list);
                }
            }
        }
        catch
        {
            // ignore load errors
        }
    }

    public async Task EndAndSaveAsync(Guid playerId, int score, string details)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player == null) return;

        var result = new AdventureResult
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Score = score,
            Date = DateTime.UtcNow,
            Details = details,
        };

        _results.Add(result);
        await PersistAllAsync();
    }

    public async Task<IReadOnlyList<AdventureResult>> GetAllGameResultsAsync()
    {
        if (!_results.Any())
        {
            await LoadAllAsync();
        }
        return await Task.FromResult(_results.AsReadOnly());
    }
}
