using Microsoft.EntityFrameworkCore;
using AuthenticationServices.Data;
using SharedModels.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core with InMemory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BlazorGameQuestDb"));

var app = builder.Build();

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
app.UseAuthorization();

app.MapControllers();

app.Run();