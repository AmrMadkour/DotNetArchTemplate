using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Auth;
using MinimalAPI.Dtos;
using MinimalAPI.Endpoints;
using Moq;

namespace Tests.MinimalAPITests.Endpoints;

public class AuthEndpointsTests
{
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:DemoUsername"] = "demo",
                ["Auth:DemoPassword"] = "demo123"
            })
            .Build();
    }

    [Fact]
    public void HandleToken_WhenCredentialsAreValid_ShouldReturnOkWithToken()
    {
        //Arrange
        var expectedToken = "signed.jwt.token";
        var expectedExpiresAtUtc = DateTime.UtcNow.AddMinutes(2);

        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        jwtTokenGeneratorMock.Setup(g => g.GenerateToken("demo")).Returns((expectedToken, expectedExpiresAtUtc));

        //Act
        var result = AuthEndpoints.HandleToken(new LoginRequestDto { Username = "demo", Password = "demo123" }, BuildConfiguration(), jwtTokenGeneratorMock.Object);

        //Assert
        var okResult = Assert.IsType<Ok<TokenResponseDto>>(result);
        Assert.Equal(expectedToken, okResult.Value!.Token);
        Assert.Equal(expectedExpiresAtUtc, okResult.Value.ExpiresAtUtc);

        //okResult.Value!.Token.Should().Be(expectedToken);
    }

    [Fact]
    public void HandleToken_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        //Arrange
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        //Act
        var result = AuthEndpoints.HandleToken(new LoginRequestDto { Username = "demo", Password = "wrong" }, BuildConfiguration(), jwtTokenGeneratorMock.Object);

        //Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
        jwtTokenGeneratorMock.Verify(g => g.GenerateToken(It.IsAny<string>()), Times.Never);

        //result.Should().BeOfType<UnauthorizedHttpResult>();
    }
}
