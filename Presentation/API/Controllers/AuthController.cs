using API.Auth;
using API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration, IJwtTokenGenerator jwtTokenGenerator) : ControllerBase
{
    // No identity exists yet at this point in the pipeline, so this is rate-limited by IP, not by user.
    [EnableRateLimiting("PerIp")]
    [HttpPost("token")]
    public ActionResult<TokenResponseDto> Token(LoginRequestDto request)
    {
        var expectedUsername = configuration["Auth:DemoUsername"];
        var expectedPassword = configuration["Auth:DemoPassword"];

        if (request.Username != expectedUsername || request.Password != expectedPassword)
        {
            return Unauthorized("Invalid credentials.");
        }

        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(request.Username!);
        return Ok(new TokenResponseDto { Token = token, ExpiresAtUtc = expiresAtUtc });
    }
}
