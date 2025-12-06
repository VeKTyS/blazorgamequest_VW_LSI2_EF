using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var authSection = builder.Configuration.GetSection("Authentication");
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:ClientId"];
        options.RequireHttpsMetadata = false;
        // Map Keycloak realm roles to ASP.NET Core roles
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                // If roles are under realm_access.roles, copy them into 'roles'
                var realmAccess = ctx.Principal?.FindFirst("realm_access");
                if (realmAccess != null)
                {
                    var json = System.Text.Json.JsonDocument.Parse(realmAccess.Value);
                    if (json.RootElement.TryGetProperty("roles", out var rolesEl) && rolesEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var identity = (System.Security.Claims.ClaimsIdentity)ctx.Principal!.Identity!;
                        foreach (var r in rolesEl.EnumerateArray())
                        {
                            identity.AddClaim(new System.Security.Claims.Claim("roles", r.GetString()!));
                        }
                    }
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole("joueur"));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Configure EF Core with InMemory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BlazorGameQuestDb"));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData(context);
}

void SeedData(ApplicationDbContext context)
{
    if (!context.Players.Any())
    {
        context.Players.AddRange(
            new SharedModels.Models.Player
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                Password = "admin123",
                IsAdmin = true,
                Health = 100,
                TotalScore = 0
            },
            new SharedModels.Models.Player
            {
                Id = Guid.NewGuid(),
                Username = "player1",
                Password = "password123",
                IsAdmin = false,
                Health = 100,
                TotalScore = 50
            }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();