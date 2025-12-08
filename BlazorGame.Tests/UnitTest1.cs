using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models;
using BlazorGame.Client.Services;
using AuthenticationServices.Data;
using AuthenticationServices.Controllers;

namespace BlazorGame.Tests
{
    public class UnitTests
    {
        // ===== Helper Methods =====
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        // ===== Original Service/Domain Tests =====
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

        // ===== InMemoryPlayerService Tests =====
        [Fact]
        public void InMemoryPlayerService_CreatePlayer_AddsPlayer()
        {
            var service = new InMemoryPlayerService();

            var player = service.CreatePlayer("test", "pass");

            Assert.NotNull(player);
            Assert.Equal("test", player.Username);
            Assert.Equal("pass", player.Password);
            Assert.Equal(0, player.TotalScore);
            Assert.Equal(100, player.Health);
            Assert.False(player.IsAdmin);
        }

        [Fact]
        public void InMemoryPlayerService_GetAllPlayers_ReturnsAllIncludingSeeded()
        {
            var service = new InMemoryPlayerService();

            var players = service.GetAllPlayers();

            Assert.NotNull(players);
            Assert.Empty(players); // parameterless ctor seeds none
        }

        [Fact]
        public void InMemoryPlayerService_GetPlayerById_ReturnsCorrectPlayer()
        {
            var service = new InMemoryPlayerService();
            var created = service.CreatePlayer("find_me", "pwd");

            var found = service.GetPlayerById(created.Id);

            Assert.NotNull(found);
            Assert.Equal("find_me", found.Username);
        }

        [Fact]
        public void InMemoryPlayerService_GetPlayerById_ReturnsNull_WhenNotFound()
        {
            var service = new InMemoryPlayerService();

            var found = service.GetPlayerById(Guid.NewGuid());

            Assert.Null(found);
        }

        [Fact]
        public void InMemoryPlayerService_DeletePlayer_RemovesPlayer()
        {
            var service = new InMemoryPlayerService();
            var created = service.CreatePlayer("delete_me", "pwd");

            var deleted = service.DeletePlayer(created.Id);

            Assert.True(deleted);
            Assert.Null(service.GetPlayerById(created.Id));
        }

        [Fact]
        public void InMemoryPlayerService_DeletePlayer_ReturnsFalse_WhenNotFound()
        {
            var service = new InMemoryPlayerService();

            var deleted = service.DeletePlayer(Guid.NewGuid());

            Assert.False(deleted);
        }

        [Fact]
        public void InMemoryPlayerService_Authenticate_ReturnsPlayer_WhenMatch()
        {
            var service = new InMemoryPlayerService();
            service.CreatePlayer("auth_test", "secret");

            var authenticated = service.Authenticate("auth_test", "secret");

            Assert.NotNull(authenticated);
            Assert.Equal("auth_test", authenticated.Username);
        }

        [Fact]
        public void InMemoryPlayerService_Authenticate_CaseInsensitive()
        {
            var service = new InMemoryPlayerService();
            service.CreatePlayer("TestUser", "secret");

            var authenticated = service.Authenticate("testuser", "secret");

            Assert.NotNull(authenticated);
        }

        [Fact]
        public void InMemoryPlayerService_Authenticate_ReturnsNull_WhenNoMatch()
        {
            var service = new InMemoryPlayerService();

            var authenticated = service.Authenticate("nobody", "wrong");

            Assert.Null(authenticated);
        }

        [Fact]
        public void InMemoryPlayerService_AddOrUpdate_AddsNewPlayer()
        {
            var service = new InMemoryPlayerService();
            var player = new Player { Username = "new", Password = "pwd", TotalScore = 10 };

            service.AddOrUpdate(player);

            var found = service.GetPlayerById(player.Id);
            Assert.NotNull(found);
            Assert.Equal(10, found.TotalScore);
        }

        [Fact]
        public void InMemoryPlayerService_AddOrUpdate_UpdatesExistingPlayer()
        {
            var service = new InMemoryPlayerService();
            var created = service.CreatePlayer("update_me", "old");

            var updated = new Player { Id = created.Id, Username = "updated", Password = "new", TotalScore = 99 };
            service.AddOrUpdate(updated);

            var found = service.GetPlayerById(created.Id);
            Assert.NotNull(found);
            Assert.Equal("updated", found.Username);
            Assert.Equal(99, found.TotalScore);
        }

        [Fact]
        public async Task InMemoryPlayerService_EndAndSaveAsync_AddsResult()
        {
            var service = new InMemoryPlayerService();
            var player = service.CreatePlayer("saver", "pwd");

            await service.EndAndSaveAsync(player.Id, 100, "Won");

            var results = await service.GetGameResultsForPlayerAsync(player.Id);
            Assert.Single(results);
            Assert.Equal(100, results.First().Score);
        }

        [Fact]
        public async Task InMemoryPlayerService_GetGameResultsForPlayerAsync_ReturnsOrdered()
        {
            var service = new InMemoryPlayerService();
            var player = service.CreatePlayer("scorer", "pwd");

            await service.EndAndSaveAsync(player.Id, 10, "first");
            await Task.Delay(10); // ensure timestamp diff
            await service.EndAndSaveAsync(player.Id, 20, "second");

            var results = await service.GetGameResultsForPlayerAsync(player.Id);

            Assert.Equal(2, results.Count);
            Assert.Equal(20, results.First().Score);
        }

        [Fact]
        public async Task InMemoryPlayerService_GetAllGameResultsAsync_ReturnsAll()
        {
            var service = new InMemoryPlayerService();
            var p1 = service.CreatePlayer("p1", "pwd");
            var p2 = service.CreatePlayer("p2", "pwd");

            await service.EndAndSaveAsync(p1.Id, 10, "p1 game");
            await service.EndAndSaveAsync(p2.Id, 20, "p2 game");

            var all = await service.GetAllGameResultsAsync();

            Assert.Equal(2, all.Count);
        }

        // ===== Database Tests =====
        [Fact]
        public async Task DbContext_CanAddAndRetrievePlayers()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var player = new Player { Username = "test", Password = "pass", TotalScore = 10 };

            ctx.Players.Add(player);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Players.FindAsync(player.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("test", retrieved.Username);
            Assert.Equal(10, retrieved.TotalScore);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveItems()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var item = new Item { Name = "Sword", Description = "Sharp", HealthEffect = 0, ScoreEffect = 10 };

            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Items.FindAsync(item.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Sword", retrieved.Name);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveMonsters()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var monster = new Monstre { Name = "Orc", Health = 50, AttackPower = 20 };

            ctx.Monsters.Add(monster);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Monsters.FindAsync(monster.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Orc", retrieved.Name);
            Assert.Equal(50, retrieved.Health);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveDonjons()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var donjon = new Donjon { Name = "Dark Castle" };

            ctx.Donjons.Add(donjon);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Donjons.FindAsync(donjon.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Dark Castle", retrieved.Name);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveSalles()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var salle = new Salle { Name = "Entrance", Description = "Dark room" };

            ctx.Salles.Add(salle);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Salles.FindAsync(salle.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Entrance", retrieved.Name);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveAdventureResults()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var result = new AdventureResult { PlayerId = Guid.NewGuid(), Score = 100, Date = DateTime.UtcNow };

            ctx.AdventureResults.Add(result);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.AdventureResults.FindAsync(result.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(100, retrieved.Score);
        }

        [Fact]
        public async Task DbContext_CanQueryAdventureResultsByPlayer()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var playerId = Guid.NewGuid();
            ctx.AdventureResults.AddRange(
                new AdventureResult { PlayerId = playerId, Score = 10, Date = DateTime.UtcNow },
                new AdventureResult { PlayerId = playerId, Score = 20, Date = DateTime.UtcNow.AddHours(-1) },
                new AdventureResult { PlayerId = Guid.NewGuid(), Score = 30, Date = DateTime.UtcNow }
            );
            await ctx.SaveChangesAsync();

            var results = await ctx.AdventureResults.Where(r => r.PlayerId == playerId).ToListAsync();

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task DbContext_CanIncludeSallesInDonjons()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var salle1 = new Salle { Name = "Room1" };
            var salle2 = new Salle { Name = "Room2" };
            var donjon = new Donjon { Name = "TestDungeon", Salles = new() { salle1, salle2 } };

            ctx.Donjons.Add(donjon);
            await ctx.SaveChangesAsync();

            var retrieved = await ctx.Donjons.Include(d => d.Salles).FirstAsync(d => d.Id == donjon.Id);
            Assert.Equal(2, retrieved.Salles.Count);
            Assert.Contains(retrieved.Salles, s => s.Name == "Room1");
        }

        [Fact]
        public async Task DbContext_CanUpdatePlayerScore()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var player = new Player { Username = "scorer", Password = "pwd", TotalScore = 0 };
            ctx.Players.Add(player);
            await ctx.SaveChangesAsync();

            player.TotalScore = 100;
            await ctx.SaveChangesAsync();

            var updated = await ctx.Players.FindAsync(player.Id);
            Assert.Equal(100, updated?.TotalScore);
        }

        [Fact]
        public async Task DbContext_CanDeletePlayer()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var player = new Player { Username = "delete", Password = "pwd" };
            ctx.Players.Add(player);
            await ctx.SaveChangesAsync();

            ctx.Players.Remove(player);
            await ctx.SaveChangesAsync();

            var deleted = await ctx.Players.FindAsync(player.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DbContext_CanDeleteItem()
        {
            using var ctx = CreateContext(Guid.NewGuid().ToString());
            var item = new Item { Name = "DeleteMe" };
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();

            ctx.Items.Remove(item);
            await ctx.SaveChangesAsync();

            var deleted = await ctx.Items.FindAsync(item.Id);
            Assert.Null(deleted);
        }

        // ===== Controllers Tests =====
        [Fact]
        public async Task PlayersController_GetPlayers_ReturnsAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Players.AddRange(
                new Player { Username = "u1", Password = "p1" },
                new Player { Username = "u2", Password = "p2" }
            );
            await ctx.SaveChangesAsync();
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayers();

            var ok = Assert.IsType<ActionResult<IEnumerable<Player>>>(result);
            var players = Assert.IsAssignableFrom<IEnumerable<Player>>(ok.Value);
            Assert.Equal(2, players.Count());
        }

        [Fact]
        public async Task PlayersController_PostPlayer_AddsPlayer()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new PlayersController(ctx);
            var newPlayer = new Player { Username = "new", Password = "pwd" };

            var result = await controller.PostPlayer(newPlayer);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var stored = Assert.IsType<Player>(created.Value);
            Assert.Equal("new", stored.Username);
            Assert.Equal(1, ctx.Players!.Count());
        }

        [Fact]
        public async Task PlayersController_GetPlayerScores_ReturnsScores()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var playerId = Guid.NewGuid();
            ctx.Players.Add(new Player { Id = playerId, Username = "test", Password = "pwd" });
            ctx.AdventureResults.AddRange(
                new AdventureResult { PlayerId = playerId, Score = 100, Date = DateTime.UtcNow },
                new AdventureResult { PlayerId = playerId, Score = 200, Date = DateTime.UtcNow }
            );
            await ctx.SaveChangesAsync();
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayerScores(playerId);

            var actionResult = Assert.IsType<ActionResult<IEnumerable<AdventureResult>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var scores = Assert.IsAssignableFrom<IEnumerable<AdventureResult>>(okResult.Value);
            Assert.Equal(2, scores.Count());
        }

        [Fact]
        public async Task PlayersController_GetPlayerScores_ReturnsEmptyForNoResults()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var playerId = Guid.NewGuid();
            ctx.Players.Add(new Player { Id = playerId, Username = "test", Password = "pwd" });
            await ctx.SaveChangesAsync();
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayerScores(playerId);

            var actionResult = Assert.IsType<ActionResult<IEnumerable<AdventureResult>>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task PlayersController_PutScore_UpdatesValue()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var player = new Player { Username = "score", Password = "pwd", TotalScore = 10 };
            ctx.Players.Add(player);
            await ctx.SaveChangesAsync();
            var controller = new PlayersController(ctx);
            var dto = new UpdateScoreDto { Score = 99 };

            var result = await controller.PutScore(player.Id, dto);

            Assert.IsType<NoContentResult>(result);
            var updated = ctx.Players.Single(p => p.Id == player.Id);
            Assert.Equal(99, updated.TotalScore);
        }

        [Fact]
        public async Task PlayersController_GetPlayerScores_NotFound_WhenNone()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var playerId = Guid.NewGuid();
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayerScores(playerId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PlayersController_GetPlayerScores_ReturnsOrdered()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var playerId = Guid.NewGuid();
            ctx.AdventureResults.AddRange(
                new AdventureResult { PlayerId = playerId, Score = 10, Date = new DateTime(2024, 1, 1) },
                new AdventureResult { PlayerId = playerId, Score = 20, Date = new DateTime(2024, 2, 1) }
            );
            await ctx.SaveChangesAsync();
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayerScores(playerId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<AdventureResult>>(ok.Value).ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal(20, list.First().Score);
        }

        [Fact]
        public async Task ItemsController_Create_Then_Get()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var item = new Item { Name = "Potion", Description = "Heal", HealthEffect = 5, ScoreEffect = 1 };

            var created = await controller.Create(item);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created);
            var stored = Assert.IsType<Item>(createdResult.Value);

            var get = await controller.Get(stored.Id);
            var ok = Assert.IsType<OkObjectResult>(get);
            var returned = Assert.IsType<Item>(ok.Value);
            Assert.Equal("Potion", returned.Name);
        }

        [Fact]
        public async Task ItemsController_GetAll_ReturnsAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Items.AddRange(
                new Item { Name = "Sword", Description = "Weapon" },
                new Item { Name = "Shield", Description = "Defense" }
            );
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Item>>(ok.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task ItemsController_Get_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ItemsController_Update_ChangesValues()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Item { Name = "Old", Description = "OldDesc" };
            ctx.Items.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);
            var model = new Item { Id = existing.Id, Name = "New", Description = "NewDesc" };

            var result = await controller.Update(existing.Id, model);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New", ctx.Items.Single().Name);
        }

        [Fact]
        public async Task ItemsController_Update_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var id = Guid.NewGuid();
            var model = new Item { Id = id, Name = "Missing" };

            var result = await controller.Update(id, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ItemsController_Update_BadRequest_OnIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var model = new Item { Name = "Mismatch" };

            var result = await controller.Update(Guid.NewGuid(), model);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ItemsController_Delete_RemovesEntity()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Item { Name = "Delete" };
            ctx.Items.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);

            var result = await controller.Delete(existing.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Items);
        }

        [Fact]
        public async Task ItemsController_Delete_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Create_Then_GetAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var donjon = new Donjon { Name = "Castle" };

            var created = await controller.Create(donjon);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created);
            var stored = Assert.IsType<Donjon>(createdResult.Value);
            Assert.Equal(donjon.Id, stored.Id);

            var listResult = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(listResult);
            var donjons = Assert.IsAssignableFrom<IEnumerable<Donjon>>(ok.Value);
            Assert.Single(donjons);
        }

        [Fact]
        public async Task DonjonsController_Get_ReturnsDonjon()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var donjon = new Donjon { Name = "Fortress" };
            ctx.Donjons.Add(donjon);
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);

            var result = await controller.Get(donjon.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Donjon>(ok.Value);
            Assert.Equal("Fortress", returned.Name);
        }

        [Fact]
        public async Task DonjonsController_Get_ReturnsNotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Update_ReturnsBadRequest_OnIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var model = new Donjon { Name = "Mismatch" };

            var result = await controller.Update(Guid.NewGuid(), model);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Update_ChangesValues()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Donjon { Name = "Old" };
            ctx.Donjons.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);
            var model = new Donjon { Id = existing.Id, Name = "New" };

            var result = await controller.Update(existing.Id, model);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New", ctx.Donjons.Single().Name);
        }

        [Fact]
        public async Task DonjonsController_Update_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var id = Guid.NewGuid();
            var model = new Donjon { Id = id, Name = "Missing" };

            var result = await controller.Update(id, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Delete_RemovesEntity()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Donjon { Name = "Delete" };
            ctx.Donjons.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);

            var result = await controller.Delete(existing.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Donjons);
        }

        [Fact]
        public async Task DonjonsController_Delete_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MonstersController_Create_Then_GetAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var monster = new Monstre { Name = "Goblin", Health = 50 };

            var created = await controller.Create(monster);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created);
            var stored = Assert.IsType<Monstre>(createdResult.Value);
            Assert.Equal(monster.Id, stored.Id);

            var list = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(list);
            var monsters = Assert.IsAssignableFrom<IEnumerable<Monstre>>(ok.Value);
            Assert.Single(monsters);
        }

        [Fact]
        public async Task MonstersController_Get_ReturnsMonster()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var monster = new Monstre { Name = "Dragon" };
            ctx.Monsters.Add(monster);
            await ctx.SaveChangesAsync();
            var controller = new MonstersController(ctx);

            var result = await controller.Get(monster.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Monstre>(ok.Value);
            Assert.Equal("Dragon", returned.Name);
        }

        [Fact]
        public async Task MonstersController_Update_ChangesValues()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Monstre { Name = "Old" };
            ctx.Monsters.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new MonstersController(ctx);
            var model = new Monstre { Id = existing.Id, Name = "New" };

            var result = await controller.Update(existing.Id, model);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New", ctx.Monsters.Single().Name);
        }

        [Fact]
        public async Task MonstersController_Update_BadRequest_OnMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var model = new Monstre { Name = "Mismatch" };

            var result = await controller.Update(Guid.NewGuid(), model);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task MonstersController_Update_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var id = Guid.NewGuid();
            var model = new Monstre { Id = id, Name = "Missing" };

            var result = await controller.Update(id, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MonstersController_Delete_RemovesEntity()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Monstre { Name = "Delete" };
            ctx.Monsters.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new MonstersController(ctx);

            var result = await controller.Delete(existing.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Monsters);
        }

        [Fact]
        public async Task MonstersController_Get_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MonstersController_Delete_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SallesController_Create_Then_GetAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var salle = new Salle { Name = "Hall" };

            var created = await controller.Create(salle);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created);
            var stored = Assert.IsType<Salle>(createdResult.Value);
            Assert.Equal(salle.Id, stored.Id);

            var list = await controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(list);
            var salles = Assert.IsAssignableFrom<IEnumerable<Salle>>(ok.Value);
            Assert.Single(salles);
        }

        [Fact]
        public async Task SallesController_Get_ReturnsSalle()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var salle = new Salle { Name = "Hall" };
            ctx.Salles.Add(salle);
            await ctx.SaveChangesAsync();
            var controller = new SallesController(ctx);

            var result = await controller.Get(salle.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Salle>(ok.Value);
            Assert.Equal("Hall", returned.Name);
        }

        [Fact]
        public async Task SallesController_Get_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SallesController_Update_ChangesValues()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Salle { Name = "Old" };
            ctx.Salles.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new SallesController(ctx);
            var model = new Salle { Id = existing.Id, Name = "New" };

            var result = await controller.Update(existing.Id, model);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("New", ctx.Salles.Single().Name);
        }

        [Fact]
        public async Task SallesController_Update_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var id = Guid.NewGuid();
            var model = new Salle { Id = id, Name = "Missing" };

            var result = await controller.Update(id, model);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SallesController_Update_BadRequest_OnIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var model = new Salle { Name = "Mismatch" };

            var result = await controller.Update(Guid.NewGuid(), model);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SallesController_Delete_RemovesEntity()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var existing = new Salle { Name = "Delete" };
            ctx.Salles.Add(existing);
            await ctx.SaveChangesAsync();
            var controller = new SallesController(ctx);

            var result = await controller.Delete(existing.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(ctx.Salles);
        }

        [Fact]
        public async Task SallesController_Delete_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AdventureResultsController_Create_Then_Get()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new AdventureResultsController(ctx);
            var resultModel = new AdventureResult { PlayerId = Guid.NewGuid(), Score = 42, Date = DateTime.UtcNow };

            var created = await controller.Create(resultModel);
            var createdResult = Assert.IsType<CreatedAtActionResult>(created);
            var stored = Assert.IsType<AdventureResult>(createdResult.Value);
            Assert.Equal(resultModel.Id, stored.Id);

            var get = await controller.Get(resultModel.Id);
            var ok = Assert.IsType<OkObjectResult>(get);
            var returned = Assert.IsType<AdventureResult>(ok.Value);
            Assert.Equal(42, returned.Score);
        }

        [Fact]
        public async Task AdventureResultsController_GetAll_ReturnsAll()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.AdventureResults.AddRange(
                new AdventureResult { PlayerId = Guid.NewGuid(), Score = 10, Date = DateTime.UtcNow },
                new AdventureResult { PlayerId = Guid.NewGuid(), Score = 20, Date = DateTime.UtcNow }
            );
            await ctx.SaveChangesAsync();
            var controller = new AdventureResultsController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var results = Assert.IsAssignableFrom<IEnumerable<AdventureResult>>(ok.Value);
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task AdventureResultsController_Get_NotFound_WhenMissing()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new AdventureResultsController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        // ===== Helper classes for AdventureService test =====
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

        // ===== Tests supplémentaires pour atteindre 80% de couverture =====

        // Tests de validation des modèles
        [Fact]
        public void Player_DefaultValues_AreCorrect()
        {
            var player = new Player();

            Assert.NotEqual(Guid.Empty, player.Id);
            Assert.Equal(string.Empty, player.Username);
            Assert.Equal(string.Empty, player.Password);
            Assert.Equal(0, player.TotalScore);
            Assert.Equal(100, player.Health);
            Assert.False(player.IsAdmin);
            Assert.NotNull(player.Inventory);
        }

        [Fact]
        public void Item_DefaultValues_AreCorrect()
        {
            var item = new Item();

            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal(string.Empty, item.Name);
            Assert.Equal(string.Empty, item.Description);
            Assert.Equal(0, item.HealthEffect);
            Assert.Equal(0, item.ScoreEffect);
        }

        [Fact]
        public void Monstre_DefaultValues_AreCorrect()
        {
            var monstre = new Monstre();

            Assert.NotEqual(Guid.Empty, monstre.Id);
            Assert.Equal(string.Empty, monstre.Name);
            Assert.Equal(100, monstre.Health);
            Assert.Equal(10, monstre.AttackPower);
            Assert.Equal(5, monstre.Defense);
            Assert.Equal(50, monstre.ScoreValue);
            Assert.NotNull(monstre.droppedItem);
            Assert.False(monstre.isBoss);
        }

        [Fact]
        public void Donjon_DefaultValues_AreCorrect()
        {
            var donjon = new Donjon();

            Assert.NotEqual(Guid.Empty, donjon.Id);
            Assert.Equal(string.Empty, donjon.Name);
            Assert.NotNull(donjon.Salles);
            Assert.Empty(donjon.Salles);
        }

        [Fact]
        public void Salle_DefaultValues_AreCorrect()
        {
            var salle = new Salle();

            Assert.NotEqual(Guid.Empty, salle.Id);
            Assert.Equal(string.Empty, salle.Name);
            Assert.Equal(string.Empty, salle.Description);
            Assert.NotNull(salle.Items);
            Assert.Empty(salle.Items);
            Assert.NotNull(salle.Monstres);
            Assert.Empty(salle.Monstres);
            Assert.NotNull(salle.ConnectedRooms);
            Assert.Empty(salle.ConnectedRooms);
        }

        [Fact]
        public void AdventureResult_DefaultValues_AreCorrect()
        {
            var result = new AdventureResult();

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(Guid.Empty, result.PlayerId);
            Assert.Equal(0, result.Score);
            Assert.False(result.IsDead);
            Assert.NotNull(result.Events);
            Assert.Empty(result.Events);
        }

        [Fact]
        public void Player_CanSetAllProperties()
        {
            var player = new Player
            {
                Username = "testuser",
                Password = "testpass",
                TotalScore = 500,
                Health = 80,
                IsAdmin = true
            };

            Assert.Equal("testuser", player.Username);
            Assert.Equal("testpass", player.Password);
            Assert.Equal(500, player.TotalScore);
            Assert.Equal(80, player.Health);
            Assert.True(player.IsAdmin);
        }

        [Fact]
        public void Item_CanSetAllProperties()
        {
            var item = new Item
            {
                Name = "Health Potion",
                Description = "Restores 50 HP",
                HealthEffect = 50,
                ScoreEffect = 10
            };

            Assert.Equal("Health Potion", item.Name);
            Assert.Equal("Restores 50 HP", item.Description);
            Assert.Equal(50, item.HealthEffect);
            Assert.Equal(10, item.ScoreEffect);
        }

        [Fact]
        public void Monstre_CanSetAllProperties()
        {
            var item = new Item { Name = "Gold" };
            var monstre = new Monstre
            {
                Name = "Dragon",
                Health = 500,
                AttackPower = 50,
                Defense = 30,
                ScoreValue = 1000,
                droppedItem = item,
                isBoss = true
            };

            Assert.Equal("Dragon", monstre.Name);
            Assert.Equal(500, monstre.Health);
            Assert.Equal(50, monstre.AttackPower);
            Assert.Equal(30, monstre.Defense);
            Assert.Equal(1000, monstre.ScoreValue);
            Assert.Equal(item, monstre.droppedItem);
            Assert.True(monstre.isBoss);
        }

        [Fact]
        public void AdventureResult_CanSetAllProperties()
        {
            var playerId = Guid.NewGuid();
            var date = DateTime.UtcNow;
            var playedAt = DateTimeOffset.UtcNow;
            var events = new[] { "Started", "Killed monster", "Found treasure" };

            var result = new AdventureResult
            {
                PlayerId = playerId,
                Date = date,
                Details = "Great adventure",
                PlayedAt = playedAt,
                Score = 250,
                IsDead = true,
                Events = events
            };

            Assert.Equal(playerId, result.PlayerId);
            Assert.Equal(date, result.Date);
            Assert.Equal("Great adventure", result.Details);
            Assert.Equal(playedAt, result.PlayedAt);
            Assert.Equal(250, result.Score);
            Assert.True(result.IsDead);
            Assert.Equal(events, result.Events);
        }

        // Tests edge cases et scénarios complexes pour controllers
        [Fact]
        public async Task ItemsController_Get_ReturnsItem()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var item = new Item { Name = "Sword", Description = "Sharp" };
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);

            var result = await controller.Get(item.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Item>(ok.Value);
            Assert.Equal("Sword", returned.Name);
        }

        [Fact]
        public async Task ItemsController_Get_ReturnsNotFound()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ItemsController_Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var item = new Item { Id = Guid.NewGuid(), Name = "Test" };

            var result = await controller.Update(Guid.NewGuid(), item);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ItemsController_Update_ReturnsNotFound_WhenItemDoesNotExist()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var id = Guid.NewGuid();
            var item = new Item { Id = id, Name = "Test" };

            var result = await controller.Update(id, item);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ItemsController_Update_UpdatesItem()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var item = new Item { Name = "Old", Description = "Desc" };
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);

            item.Name = "New";
            var result = await controller.Update(item.Id, item);

            Assert.IsType<NoContentResult>(result);
            var updated = await ctx.Items.FindAsync(item.Id);
            Assert.Equal("New", updated?.Name);
        }

        [Fact]
        public async Task MonstersController_Get_ReturnsNotFound()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MonstersController_Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var monster = new Monstre { Id = Guid.NewGuid(), Name = "Test" };

            var result = await controller.Update(Guid.NewGuid(), monster);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task MonstersController_Update_ReturnsNotFound_WhenMonsterDoesNotExist()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var id = Guid.NewGuid();
            var monster = new Monstre { Id = id, Name = "Test" };

            var result = await controller.Update(id, monster);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MonstersController_Update_UpdatesMonster()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var monster = new Monstre { Name = "Old", Health = 50 };
            ctx.Monsters.Add(monster);
            await ctx.SaveChangesAsync();
            var controller = new MonstersController(ctx);

            monster.Name = "New";
            var result = await controller.Update(monster.Id, monster);

            Assert.IsType<NoContentResult>(result);
            var updated = await ctx.Monsters.FindAsync(monster.Id);
            Assert.Equal("New", updated?.Name);
        }

        [Fact]
        public async Task DonjonsController_Get_ReturnsDonjonWithSalles()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var salle = new Salle { Name = "Room1" };
            var donjon = new Donjon { Name = "Castle", Salles = new() { salle } };
            ctx.Donjons.Add(donjon);
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);

            var result = await controller.Get(donjon.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Donjon>(ok.Value);
            Assert.Equal("Castle", returned.Name);
            Assert.Single(returned.Salles);
        }

        [Fact]
        public async Task DonjonsController_Get_ReturnsNotFound()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var donjon = new Donjon { Id = Guid.NewGuid(), Name = "Test" };

            var result = await controller.Update(Guid.NewGuid(), donjon);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Update_ReturnsNotFound_WhenDonjonDoesNotExist()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var id = Guid.NewGuid();
            var donjon = new Donjon { Id = id, Name = "Test" };

            var result = await controller.Update(id, donjon);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DonjonsController_Update_UpdatesDonjon()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var donjon = new Donjon { Name = "Old" };
            ctx.Donjons.Add(donjon);
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);

            donjon.Name = "New";
            var result = await controller.Update(donjon.Id, donjon);

            Assert.IsType<NoContentResult>(result);
            var updated = await ctx.Donjons.FindAsync(donjon.Id);
            Assert.Equal("New", updated?.Name);
        }

        [Fact]
        public async Task SallesController_Get_ReturnsNotFound()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SallesController_Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var salle = new Salle { Id = Guid.NewGuid(), Name = "Test" };

            var result = await controller.Update(Guid.NewGuid(), salle);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SallesController_Update_ReturnsNotFound_WhenSalleDoesNotExist()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var id = Guid.NewGuid();
            var salle = new Salle { Id = id, Name = "Test" };

            var result = await controller.Update(id, salle);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SallesController_Update_UpdatesSalle()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var salle = new Salle { Name = "Old", Description = "Desc" };
            ctx.Salles.Add(salle);
            await ctx.SaveChangesAsync();
            var controller = new SallesController(ctx);

            salle.Name = "New";
            var result = await controller.Update(salle.Id, salle);

            Assert.IsType<NoContentResult>(result);
            var updated = await ctx.Salles.FindAsync(salle.Id);
            Assert.Equal("New", updated?.Name);
        }

        [Fact]
        public async Task ItemsController_GetAll_ReturnsMultipleItems()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Items.AddRange(
                new Item { Name = "Item1" },
                new Item { Name = "Item2" },
                new Item { Name = "Item3" }
            );
            await ctx.SaveChangesAsync();
            var controller = new ItemsController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<Item>>(ok.Value);
            Assert.Equal(3, items.Count());
        }

        [Fact]
        public async Task MonstersController_GetAll_ReturnsMultipleMonsters()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Monsters.AddRange(
                new Monstre { Name = "Monster1" },
                new Monstre { Name = "Monster2" },
                new Monstre { Name = "Monster3" }
            );
            await ctx.SaveChangesAsync();
            var controller = new MonstersController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var monsters = Assert.IsAssignableFrom<IEnumerable<Monstre>>(ok.Value);
            Assert.Equal(3, monsters.Count());
        }

        [Fact]
        public async Task DonjonsController_GetAll_ReturnsMultipleDonjons()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Donjons.AddRange(
                new Donjon { Name = "Dungeon1" },
                new Donjon { Name = "Dungeon2" }
            );
            await ctx.SaveChangesAsync();
            var controller = new DonjonsController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var donjons = Assert.IsAssignableFrom<IEnumerable<Donjon>>(ok.Value);
            Assert.Equal(2, donjons.Count());
        }

        [Fact]
        public async Task SallesController_GetAll_ReturnsMultipleSalles()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            ctx.Salles.AddRange(
                new Salle { Name = "Room1" },
                new Salle { Name = "Room2" },
                new Salle { Name = "Room3" }
            );
            await ctx.SaveChangesAsync();
            var controller = new SallesController(ctx);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var salles = Assert.IsAssignableFrom<IEnumerable<Salle>>(ok.Value);
            Assert.Equal(3, salles.Count());
        }

        [Fact]
        public async Task PlayersController_GetPlayers_ReturnsEmptyList()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new PlayersController(ctx);

            var result = await controller.GetPlayers();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Player>>>(result);
            var players = Assert.IsAssignableFrom<IEnumerable<Player>>(actionResult.Value);
            Assert.Empty(players);
        }

        [Fact]
        public async Task ItemsController_Create_AddsNewItem()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new ItemsController(ctx);
            var item = new Item { Name = "New Item", Description = "Test" };

            var result = await controller.Create(item);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var stored = Assert.IsType<Item>(created.Value);
            Assert.Equal("New Item", stored.Name);
            Assert.Equal(1, ctx.Items.Count());
        }

        [Fact]
        public async Task MonstersController_Create_AddsNewMonster()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new MonstersController(ctx);
            var monster = new Monstre { Name = "New Monster", Health = 75 };

            var result = await controller.Create(monster);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var stored = Assert.IsType<Monstre>(created.Value);
            Assert.Equal("New Monster", stored.Name);
            Assert.Equal(1, ctx.Monsters.Count());
        }

        [Fact]
        public async Task DonjonsController_Create_AddsNewDonjon()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new DonjonsController(ctx);
            var donjon = new Donjon { Name = "New Dungeon" };

            var result = await controller.Create(donjon);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var stored = Assert.IsType<Donjon>(created.Value);
            Assert.Equal("New Dungeon", stored.Name);
            Assert.Equal(1, ctx.Donjons.Count());
        }

        [Fact]
        public async Task SallesController_Create_AddsNewSalle()
        {
            var ctx = CreateContext(Guid.NewGuid().ToString());
            var controller = new SallesController(ctx);
            var salle = new Salle { Name = "New Room", Description = "Test" };

            var result = await controller.Create(salle);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var stored = Assert.IsType<Salle>(created.Value);
            Assert.Equal("New Room", stored.Name);
            Assert.Equal(1, ctx.Salles.Count());
        }

    }
}
