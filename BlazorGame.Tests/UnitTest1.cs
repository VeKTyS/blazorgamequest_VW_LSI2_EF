using Xunit;
using SharedModels.Models;


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
}