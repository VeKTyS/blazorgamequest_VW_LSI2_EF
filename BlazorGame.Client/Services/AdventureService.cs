using System.Net.Http.Json;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class AdventureService : IAdventureService
{
    private readonly IDungeonGenerator _generator;
    private readonly HttpClient _http;
    private readonly IItemService _itemService;
    private Donjon? _currentDonjon;
    private int _currentIndex = 0;
    private string _lastEvent = string.Empty;

    public AdventureService(IDungeonGenerator generator, HttpClient http, IItemService itemService)
    {
        _generator = generator;
        _http = http;
        _itemService = itemService;
    }

    public Player CurrentPlayer { get; private set; } = new Player();

    public Salle? CurrentRoom => _currentDonjon?.Salles.ElementAtOrDefault(_currentIndex);

    public string LastEvent => _lastEvent;

    public bool IsFinished { get; private set; }

    public void StartAdventure(Player player, int rooms = 5)
    {
        CurrentPlayer = player;
        _currentDonjon = _generator.Generate(rooms);
        _currentIndex = 0;
        IsFinished = false;
        _lastEvent = $"Début du donjon '{_currentDonjon.Name}'";
    }

    private void AddEvent(string e) { _lastEvent = e; }

    public void Explore()
    {
        if (IsFinished || _currentDonjon == null) return;
        // Move to next room if possible
        if (_currentIndex < _currentDonjon.Salles.Count - 1)
        {
            _currentIndex++;
            AddEvent($"Vous avancez vers {_currentDonjon.Salles[_currentIndex].Name}.");
        }
        else
        {
            AddEvent("Vous atteignez la fin du donjon.");
            IsFinished = true;
        }
    }

    public void Search()
    {
        var room = CurrentRoom;
        if (room == null) return;

        // First check for items lying in the current room (e.g. dropped by monsters)
        var roomItem = room.Items.FirstOrDefault();
        if (roomItem != null)
        {
            // take the item from the room
            room.Items.Remove(roomItem);
            CurrentPlayer.Inventory.Add(roomItem);
            CurrentPlayer.TotalScore += roomItem.ScoreEffect;
            AddEvent($"Vous récupérez {roomItem.Name} (+{roomItem.ScoreEffect} pts, {roomItem.HealthEffect} PV) dans la salle.");
            return;
        }

        // If no room item, try to take an item from the global EF item store (shared inventory)
        var item = _itemService.TakeRandomItem();
        if (item != null)
        {
            CurrentPlayer.Inventory.Add(item);
            CurrentPlayer.TotalScore += item.ScoreEffect;
            AddEvent($"Vous récupérez {item.Name} (+{item.ScoreEffect} pts, {item.HealthEffect} PV).");
            return;
        }

        // maybe find small treasure
        if (new Random().NextDouble() < 0.3)
        {
            var gain = 10 + new Random().Next(0, 50);
            CurrentPlayer.TotalScore += gain;
            AddEvent($"Vous trouvez un petit trésor (+{gain} pts).\n");
            return;
        }

        AddEvent("Rien trouvé.");
    }

    public void Rest()
    {
        CurrentPlayer.Health = Math.Min(100, CurrentPlayer.Health + 10);
        AddEvent("Vous vous reposez et récupérez 10 PV.");
    }

    public void Attack()
    {
        var room = CurrentRoom;
        if (room == null || !room.Monstres.Any())
        {
            AddEvent("Aucun ennemi ici.");
            return;
        }

        var monster = room.Monstres.First();

        // Simple fight: player always deals fixed damage subject to randomness
        var playerDamage = 20 + new Random().Next(0, 20);
        monster.Health -= playerDamage;
        AddEvent($"Vous infligez {playerDamage} dégâts à {monster.Name}.");

        if (monster.Health <= 0)
        {
            CurrentPlayer.TotalScore += monster.ScoreValue;
            // if the monster had a dropped item, place it in the room (player must search to pick it up)
            if (monster.droppedItem != null)
            {
                room.Items.Add(monster.droppedItem);
                AddEvent($"{monster.Name} laisse tomber {monster.droppedItem.Name}.");
            }
            room.Monstres.Remove(monster);
            AddEvent($"{monster.Name} vaincu ! +{monster.ScoreValue} pts.");
            return;
        }

        // Monster retaliates
        var monsterDamage = Math.Max(0, monster.AttackPower - new Random().Next(0, 5));
        CurrentPlayer.Health -= monsterDamage;
        AddEvent($"{monster.Name} riposte et vous inflige {monsterDamage} dégâts.");

        if (CurrentPlayer.Health <= 0)
        {
            AddEvent("Vous êtes mort...");
            IsFinished = true;
        }
    }

    public void Flee()
    {
        // Fleeing moves back one room if possible and costs some health
        if (_currentIndex > 0)
        {
            _currentIndex--;
            CurrentPlayer.Health -= 5;
            AddEvent($"Vous fuyez vers {_currentDonjon?.Salles[_currentIndex].Name} et perdez 5 PV.");
        }
        else
        {
            CurrentPlayer.Health -= 10;
            AddEvent("Vous tentez de fuir mais vous êtes blessé (-10 PV).");
        }

        if (CurrentPlayer.Health <= 0)
        {
            IsFinished = true;
            AddEvent("Vous êtes mort en fuyant...");
        }
    }

    public async Task<AdventureResult> EndAndSaveAsync()
    {
        IsFinished = true;

        var result = new AdventureResult
        {
            PlayerId = CurrentPlayer.Id,
            Score = CurrentPlayer.TotalScore,
            IsDead = CurrentPlayer.Health <= 0,
            Events = new[] { LastEvent }
        };

        try
        {
            // Post adventure result
            await _http.PostAsJsonAsync("api/AdventureResults", result);

            // Optionally save dungeon
            if (_currentDonjon != null)
            {
                await _http.PostAsJsonAsync("api/Donjons", _currentDonjon);
            }
        }
        catch
        {
            // ignore network errors in client mode
        }

        return result;
    }
}
