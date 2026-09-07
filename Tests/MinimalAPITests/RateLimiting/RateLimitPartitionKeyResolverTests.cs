using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MinimalAPI.RateLimiting;

namespace Tests.MinimalAPITests.RateLimiting;

public class RateLimitPartitionKeyResolverTests
{
    [Fact]
    public void ResolveUserOrIpKey_WhenUserIsAuthenticated_ShouldReturnNameIdentifierClaim()
    {
        //Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "demo")], "TestAuthType");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        //Act
        var key = RateLimitPartitionKeyResolver.ResolveUserOrIpKey(httpContext);

        //Assert
        Assert.Equal("demo", key);

        //key.Should().Be("demo");
    }

    [Fact]
    public void ResolveUserOrIpKey_WhenUserIsNotAuthenticated_ShouldReturnRemoteIpAddress()
    {
        //Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        //Act
        var key = RateLimitPartitionKeyResolver.ResolveUserOrIpKey(httpContext);

        //Assert
        Assert.Equal("127.0.0.1", key);

        //key.Should().Be("127.0.0.1");
    }
}
