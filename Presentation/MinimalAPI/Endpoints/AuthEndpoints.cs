using MinimalAPI.Auth;
using MinimalAPI.Dtos;

namespace MinimalAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        // No identity exists yet at this point in the pipeline, so this is rate-limited by IP, not by user.
        group.MapPost("/token", HandleToken).RequireRateLimiting("PerIp");
    }

    // Pulled out of the route lambda so it's unit-testable on its own, same reasoning as RateLimitPartitionKeyResolver.
    public static IResult HandleToken(LoginRequestDto request, IConfiguration configuration, IJwtTokenGenerator jwtTokenGenerator)
    {
        var expectedUsername = configuration["Auth:DemoUsername"];
        var expectedPassword = configuration["Auth:DemoPassword"];

        if (request.Username != expectedUsername || request.Password != expectedPassword)
        {
            return Results.Unauthorized();
        }

        var (token, expiresAtUtc) = jwtTokenGenerator.GenerateToken(request.Username!);
        return Results.Ok(new TokenResponseDto { Token = token, ExpiresAtUtc = expiresAtUtc });
    }
}
