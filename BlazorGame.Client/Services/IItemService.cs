using SharedModels.Models;

namespace BlazorGame.Client.Services;

public interface IItemService
{
    IReadOnlyList<Item> GetAllItems();
    Item? GetById(Guid id);
    Item AddItem(Item item);
    bool RemoveItem(Guid id);
    Item? TakeRandomItem();
}
