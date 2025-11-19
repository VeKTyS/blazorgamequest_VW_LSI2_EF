using System.Net.Http.Json;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class DonjonService
{
    private readonly HttpClient _http;
    public DonjonService(HttpClient http) => _http = http;

    public async Task<Donjon?> GenerateAsync(int rooms = 10, int? seed = null)
    {
        var url = $"api/donjons/generate?rooms={rooms}" + (seed.HasValue ? $"&seed={seed.Value}" : "");
        var resp = await _http.PostAsJsonAsync(url, new { }); // body ignored by API
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<Donjon>();
    }

    public async Task<List<Donjon>?> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<Donjon>>("api/donjons");
    }
}