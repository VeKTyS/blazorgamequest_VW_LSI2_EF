using SharedModels.Models;

namespace BlazorGame.Client.Services;

public interface IDungeonGenerator
{
    Donjon Generate(int roomsCount = 5, string? name = null);
}
