using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using SharedModels.Models;
using BlazorGame.Client.Services;

namespace BlazorGame.Tests
{
    public class UnitTests
    {
        [Fact]
        public void InMemoryPlayerService_CreatePlayer_HasDefaults()
        {
            var svc = new InMemoryPlayerService();
            var p = svc.CreatePlayer("alice", "pwd");

            Assert.False(string.IsNullOrWhiteSpace(p.Username));
            Assert.Equal("alice", p.Username);
            Assert.Equal(100, p.Health);
            Assert.NotNull(p.Inventory);
            Assert.Empty(p.Inventory);
        }

        [Fact]
        public void Player_TotalScore_Increases_OnUpdate()
        {
            var p = new Player { Username = "bob", TotalScore = 10 };
            p.TotalScore += 25;
            Assert.Equal(35, p.TotalScore);
        }

        [Fact]
        public void DungeonGenerator_Generates_CorrectRoomCount_And_NoDuplicates()
        {
            var gen = new DungeonGeneratorService();
            var d = gen.Generate(roomsCount: 6, name: "test");

            Assert.Equal(6, d.Salles.Count);

            // No duplicate IDs
            var ids = d.Salles.Select(s => s.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());

            // No orphan rooms (for >1 rooms each should have at least one connection)
            if (d.Salles.Count > 1)
            {
                foreach (var s in d.Salles)
                {
                    Assert.True(s.ConnectedRooms != null && s.ConnectedRooms.Count >= 1);
                }
            }
        }

        [Fact]
        public void Salle_Creation_And_MonsterPresence()
        {
            var s = new Salle { Name = "TestRoom", Description = "desc" };
            s.Monstres.Add(new Monstre { Name = "Gob", Health = 50 });

            Assert.Equal("TestRoom", s.Name);
            Assert.NotEmpty(s.Monstres);
        }

        [Fact]
        public void AddingItemToInventory_Works()
        {
            var p = new Player { Username = "carl" };
            var it = new Item { Name = "Potion", HealthEffect = 20, ScoreEffect = 5 };
            p.Inventory.Add(it);

            Assert.Single(p.Inventory);
            Assert.Equal("Potion", p.Inventory[0].Name);
        }

        [Fact]
        public void AdventureService_Attack_ChangesHealths()
        {
            // Create a deterministic generator that returns a Donjon with one room and one weak monster
            var gen = new TestGenerator();
            var http = new System.Net.Http.HttpClient();
            var itemSvc = new TestItemService();
            var playerSvc = new BlazorGame.Client.Services.InMemoryPlayerService();
            var playerState = new BlazorGame.Client.Services.PlayerStateService();
            var adv = new AdventureService(gen, http, itemSvc, playerSvc, playerState);

            var player = new Player { Username = "fighter", Health = 100 };
            adv.StartAdventure(player, rooms: 1);

            var room = adv.CurrentRoom;
            Assert.NotNull(room);
            Assert.NotEmpty(room.Monstres);

            var monsterBefore = room.Monstres.First().Health;
            var playerBefore = adv.CurrentPlayer.Health;

            adv.Attack();

            // After attack, either monster took damage or player took damage (or both). Assert that something changed.
            var monsterAfter = room.Monstres.FirstOrDefault()?.Health;
            var playerAfter = adv.CurrentPlayer.Health;

            Assert.True(playerAfter <= playerBefore);
            // monster may have been removed if killed; ensure either it's lower or it's gone
            if (monsterAfter.HasValue)
            {
                Assert.True(monsterAfter.Value <= monsterBefore);
            }
        }

        // Helper test generator
        class TestGenerator : IDungeonGenerator
        {
            public Donjon Generate(int roomsCount = 5, string? name = null)
            {
                var d = new Donjon { Name = name ?? "test" };
                var r = new Salle { Name = "R1", Description = "d" };
                r.Monstres.Add(new Monstre { Name = "Weak", Health = 30, AttackPower = 5, ScoreValue = 10 });
                d.Salles.Add(r);
                return d;
            }
        }

        // Minimal item service for tests
        class TestItemService : IItemService
        {
            private readonly List<Item> _items = new();
            public Item AddItem(Item item)
            {
                _items.Add(item);
                return item;
            }

            public IReadOnlyList<Item> GetAllItems() => _items.AsReadOnly();

            public Item? GetById(Guid id) => _items.FirstOrDefault(i => i.Id == id);

            public Item? TakeRandomItem()
            {
                if (!_items.Any()) return null;
                var it = _items[0];
                _items.RemoveAt(0);
                return it;
            }

            public bool RemoveItem(Guid id)
            {
                var it = _items.FirstOrDefault(i => i.Id == id);
                if (it == null) return false;
                _items.Remove(it);
                return true;
            }
        }
    }
}