using API.Auth;
using API.Controllers;
using API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Tests.APITests.Controllers;

public class AuthControllerTests
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
    public void Token_WhenCredentialsAreValid_ShouldReturnOkWithToken()
    {
        //Arrange
        var expectedToken = "signed.jwt.token";
        var expectedExpiresAtUtc = DateTime.UtcNow.AddMinutes(2);

        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        jwtTokenGeneratorMock.Setup(g => g.GenerateToken("demo")).Returns((expectedToken, expectedExpiresAtUtc));
        var authController = new AuthController(BuildConfiguration(), jwtTokenGeneratorMock.Object);

        //Act
        var response = authController.Token(new LoginRequestDto { Username = "demo", Password = "demo123" });

        //Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var tokenResponse = Assert.IsType<TokenResponseDto>(okResult.Value);
        Assert.Equal(expectedToken, tokenResponse.Token);
        Assert.Equal(expectedExpiresAtUtc, tokenResponse.ExpiresAtUtc);

        //var okResultFa = response.Result.Should().BeOfType<OkObjectResult>().Which;
        //okResultFa.Value.Should().BeOfType<TokenResponseDto>().Which.Token.Should().Be(expectedToken);
    }

    [Fact]
    public void Token_WhenCredentialsAreInvalid_ShouldReturnUnauthorized()
    {
        //Arrange
        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var authController = new AuthController(BuildConfiguration(), jwtTokenGeneratorMock.Object);

        //Act
        var response = authController.Token(new LoginRequestDto { Username = "demo", Password = "wrong" });

        //Assert
        Assert.IsType<UnauthorizedObjectResult>(response.Result);
        jwtTokenGeneratorMock.Verify(g => g.GenerateToken(It.IsAny<string>()), Times.Never);

        //response.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
