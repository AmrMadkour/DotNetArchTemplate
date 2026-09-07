using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Auth;

namespace Tests.MinimalAPITests.Auth;

public class JwtTokenGeneratorTests
{
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:SigningKey"] = "test-signing-key-thats-long-enough-1234567890",
                ["Jwt:ExpiryMinutes"] = "2"
            })
            .Build();
    }

    [Fact]
    public void GenerateToken_WhenCalled_ShouldReturnTokenWithExpectedSubjectClaimAndFutureExpiry()
    {
        //Arrange
        var jwtTokenGenerator = new JwtTokenGenerator(BuildConfiguration());

        //Act
        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken("demo");

        //Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("demo", jwt.Subject);
        Assert.True(expiresAtUtc > DateTime.UtcNow);

        //jwt.Subject.Should().Be("demo");
        //expiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }
}
