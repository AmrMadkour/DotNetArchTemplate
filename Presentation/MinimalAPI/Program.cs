using System.Text;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MinimalAPI.Auth;
using MinimalAPI.Endpoints;
using MinimalAPI.Middleware;
using MinimalAPI.RateLimiting;
using NLog.Web;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configured via nlog.config rather than appsettings — Console for local viewing,
// rolling daily file under logs/ for anything you want to grep back through later.
builder.Host.UseNLog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Required for Swashbuckle to discover minimal API endpoints (no AddControllers() here to register it for us)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// Validates JWTs issued by this same app's own /auth/token endpoint — same signing key, no external identity provider.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Keyed by the authenticated user's claim (set once JWT validation has run) for protected endpoints.
    options.AddPolicy("PerUser", context =>
        RateLimitPartition.GetFixedWindowLimiter(RateLimitPartitionKeyResolver.ResolveUserOrIpKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(30),
            QueueLimit = 0
        }));

    // No identity exists yet before a token is issued, so the token endpoint is keyed by IP instead.
    options.AddPolicy("PerIp", context =>
        RateLimitPartition.GetFixedWindowLimiter(RateLimitPartitionKeyResolver.ResolveIpKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(30),
            QueueLimit = 0
        }));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    SuppressDiagnosticsCallback = _ => true
});

app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Launch the app and navigate to https://localhost:{PORT}/openapi/v1.json
    app.MapOpenApi();
    //Launch the app and navigate to https://localhost:{PORT}/swagger/v1/swagger.json
    app.UseSwagger();
    //Launch the app and navigate to https://localhost:{PORT}/swagger/index.html
    app.UseSwaggerUI();
    //Launch the app and navigate to https://localhost:{PORT}/scalar
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapOrderEndpoints();
app.MapDiagnosticsEndpoints();
app.MapAuthEndpoints();

app.Run();

