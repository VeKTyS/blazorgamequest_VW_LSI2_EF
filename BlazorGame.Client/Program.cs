using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using Microsoft.EntityFrameworkCore;
using BlazorGame.Client.Data;
using BlazorGame.Client.Services;
using SharedModels.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure EF Core with InMemory database
builder.Services.AddDbContextFactory<GameDbContext>(options =>
	options.UseInMemoryDatabase("BlazorGameClientDb"));

// Register the EF-based player service
builder.Services.AddScoped<IPlayerService, EFPlayerService>();

// Register dungeon & adventure services
builder.Services.AddScoped<IDungeonGenerator, DungeonGeneratorService>();
builder.Services.AddScoped<IAdventureService, AdventureService>();

var host = builder.Build();

// Seed the database with initial data
await using (var scope = host.Services.CreateAsyncScope())
{
	var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<GameDbContext>>();
	await using var context = await contextFactory.CreateDbContextAsync();
    
	if (!context.Players.Any())
	{
		context.Players.AddRange(
			new Player
			{
				Id = Guid.NewGuid(),
				Username = "admin",
				Password = "admin123",
				IsAdmin = true,
				Health = 100,
				TotalScore = 0
			},
			new Player
			{
				Id = Guid.NewGuid(),
				Username = "player1",
				Password = "password123",
				IsAdmin = false,
				Health = 100,
				TotalScore = 50
			}
		);
		await context.SaveChangesAsync();
	}
}

await host.RunAsync();
