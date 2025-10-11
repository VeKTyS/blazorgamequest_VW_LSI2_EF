using Xunit;
using SharedModels.Models;
using BlazorGame.Client.Services;


namespace BlazorGame.Tests;


public class SampleTests
{
    [Fact]
    public void Player_Defaults_Work()
    {
        var p = new Player { Username = "test" };
        Assert.NotEqual(Guid.Empty, p.Id);
        Assert.Equal("test", p.Username);
        Assert.Equal(0, p.TotalScore);
    }
    
    [Fact]
    public void InMemoryPlayerService_Create_And_Delete()
    {
        var svc = new InMemoryPlayerService();
        var before = svc.GetAllPlayers().Count;
        var created = svc.CreatePlayer("toto", "pwd");
        Assert.Contains(svc.GetAllPlayers(), p => p.Username == "toto");
        var afterCreate = svc.GetAllPlayers().Count;
        Assert.Equal(before + 1, afterCreate);
        var deleted = svc.DeletePlayer(created.Id);
        Assert.True(deleted);
        var afterDelete = svc.GetAllPlayers().Count;
        Assert.Equal(before, afterDelete);
    }
}