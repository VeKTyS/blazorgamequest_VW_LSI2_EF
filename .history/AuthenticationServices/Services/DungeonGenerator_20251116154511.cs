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

        // génère un donjon en choisissant roomCount templates uniques (ou moins si pas assez)
        public Donjon Generate(int roomCount = 10, int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            var templates = _db.RoomTemplates.AsNoTracking().ToList();
            if (!templates.Any()) throw new InvalidOperationException("No room templates available.");

            // weighted sample without replacement
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

            var donjon = new Donjon
            {
                Name = $"Donjon_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Description = "Donjon généré procéduralement",
                Salles = new List<Salle>()
            };

            int index = 0;
            foreach (var tmpl in chosen)
            {
                // difficulté interpolée entre min/max selon position dans le donjon
                var difficulty = tmpl.MinDifficulty;
                if (tmpl.MaxDifficulty > tmpl.MinDifficulty)
                {
                    var t = (double)index / Math.Max(1, roomCount - 1);
                    difficulty = tmpl.MinDifficulty + (int)Math.Round(t * (tmpl.MaxDifficulty - tmpl.MinDifficulty));
                }

                var salle = new Salle
                {
                    Name = tmpl.Name,
                    Description = tmpl.Description,
                    // si votre Salle a des propriétés Type/TemplateId/Difficulty, vous pouvez les remplir ici.
                    // On copie au minimum le contenu visible (Name/Description). Vous pouvez étendre Salle pour inclure TemplateId/Difficulty.
                    Items = new List<Item>(),
                    Monstres = new List<Monstre>()
                };

                // instancier quelques éléments/monstres depuis les pools si présents
                foreach (var mid in tmpl.PossibleMonsterIds)
                {
                    var m = _db.Monsters.Find(mid);
                    if (m != null)
                    {
                        // clone minimal (éviter partager l'entité tracked)
                        var clone = new Monstre
                        {
                            Id = Guid.NewGuid(),
                            Name = m.Name,
                            Health = m.Health,
                            AttackPower = m.AttackPower,
                            Defense = m.Defense,
                            ScoreValue = m.ScoreValue
                        };
                        salle.Monstres.Add(clone);
                    }
                }

                foreach (var iid in tmpl.PossibleItemIds)
                {
                    var it = _db.Items.Find(iid);
                    if (it != null)
                    {
                        var clone = new Item
                        {
                            Id = Guid.NewGuid(),
                            Name = it.Name,
                            Description = it.Description,
                            HealthEffect = it.HealthEffect,
                            ScoreValue = it.ScoreValue,
                            //rajout valeur 
                        };
                        salle.Items.Add(clone);
                    }
                }

                donjon.Salles.Add(salle);
                index++;
            }

            _db.Donjons.Add(donjon);
            _db.SaveChanges();
            return donjon;
        }
    }
}