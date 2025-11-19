using System;
using System.Linq;
using System.Collections.Generic;
using AuthenticationServices.Data;
using SharedModels.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationServices.Services
{
    public class DungeonGenerator
    {
        private readonly ApplicationDbContext _db;
        public DungeonGenerator(ApplicationDbContext db) => _db = db;

        public Donjon Generate(int roomCount = 10, int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            var templates = _db.Set<RoomTemplate>().ToList();
            if (!templates.Any()) throw new InvalidOperationException("No room templates available.");

            // Weighted random sample without replacement
            var chosen = new List<RoomTemplate>();
            var pool = new List<RoomTemplate>(templates);
            while (chosen.Count < roomCount && pool.Count > 0)
            {
                var totalWeight = pool.Sum(t => t.Weight);
                var pick = rng.NextDouble() * totalWeight;
                double acc = 0;
                RoomTemplate selected = pool[0];
                foreach (var t in pool)
                {
                    acc += t.Weight;
                    if (pick <= acc) { selected = t; break; }
                }
                chosen.Add(selected);
                pool.Remove(selected);
            }

            var dungeon = new Donjon
            {
                Name = $"Donjon_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Salles = chosen.Select((t, index) => new Salle
                {
                    Name = t.Name,
                    Description = t.Description,
                    // map type, set difficulty within range
                    // Assumes Salle has Type and Difficulty fields — adaptez si nécessaire
                    // ...
                }).ToList()
            };

            _db.Donjons.Add(dungeon);
            _db.SaveChanges();
            return dungeon;
        }
    }
}