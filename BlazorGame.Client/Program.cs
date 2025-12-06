using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Http;

using BlazorGame.Client;
using Microsoft.EntityFrameworkCore;
using BlazorGame.Client.Data;
using BlazorGame.Client.Services;
using SharedModels.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Default client for same-origin requests (static site)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// Auth-aware HttpClient to call API in Docker (includes Keycloak access token)
builder.Services.AddTransient<AuthorizationMessageHandler>(sp =>
{
	var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
	handler.ConfigureHandler(
		authorizedUrls: new[] { "http://localhost:5000" }
	);
	return handler;
});
builder.Services.AddTransient<BlazorGame.Client.Services.ApiLoggingHandler>();

builder.Services.AddHttpClient("api", client =>
{
	client.BaseAddress = new Uri("http://localhost:5000/");
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>())
.AddHttpMessageHandler<BlazorGame.Client.Services.ApiLoggingHandler>();

// Configure EF Core with InMemory database
builder.Services.AddDbContextFactory<GameDbContext>(options =>
	options.UseInMemoryDatabase("BlazorGameClientDb"));

// Register the EF-based player service
builder.Services.AddScoped<IPlayerService, EFPlayerService>();

// Player state (keeps logged-in player across pages)
builder.Services.AddScoped<IPlayerStateService, PlayerStateService>();

// Register dungeon & adventure services
builder.Services.AddScoped<IDungeonGenerator, DungeonGeneratorService>();
builder.Services.AddScoped<IAdventureService, AdventureService>();
// Register item service
builder.Services.AddScoped<IItemService, EFItemService>();
builder.Services.AddOidcAuthentication(options =>
{
	builder.Configuration.Bind("Oidc", options.ProviderOptions);
	// Redirect back into the SPA after login/logout
	var baseUri = new Uri(builder.HostEnvironment.BaseAddress).ToString();
	options.ProviderOptions.RedirectUri = baseUri;
	options.ProviderOptions.PostLogoutRedirectUri = baseUri + "loggedout";
});

// Log configured OIDC settings to browser console
Console.WriteLine($"OIDC Authority: {builder.Configuration["Oidc:Authority"]}, ClientId: {builder.Configuration["Oidc:ClientId"]}");

var host = builder.Build();

// Log authentication state changes (to verify Keycloak sign-in)
var authProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
authProvider.AuthenticationStateChanged += async task =>
{
	var state = await task;
	var user = state.User;
	Console.WriteLine($"Auth state changed. IsAuthenticated={user.Identity?.IsAuthenticated}, Name={user.Identity?.Name}");
	if (user.Identity?.IsAuthenticated == true)
	{
		var roles = string.Join(',', user.FindAll("roles").Select(c => c.Value));
		Console.WriteLine($"User roles: {roles}");
	}
};

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

	// Seed default items
	if (!context.Items.Any())
	{
		context.Items.AddRange(
			new Item { Name = "Potion Mineure", Description = "Soigne 10 PV", HealthEffect = 10, ScoreEffect = 5 },
			new Item { Name = "Petit Trésor", Description = "Un petit sac de pièces", HealthEffect = 0, ScoreEffect = 25 }
		);
		await context.SaveChangesAsync();
	}
}

await host.RunAsync();
