using System.Text;
using API.Auth;
using API.Middleware;
using API.RateLimiting;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configured in code rather than appsettings for simplicity — Console for local viewing,
// rolling daily file under logs/ for anything you want to grep back through later.
builder.Host.UseSerilog((_, configuration) => configuration
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Allows the Presentation/WebApp React dev server (a different origin) to call this API.
const string ReactDevCorsPolicy = "ReactDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ReactDevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

// Validates JWTs issued by this same app's own /api/Auth/token endpoint — same signing key, no external identity provider.
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
    //Launch the app and navigate to https://localhost:{PORT}/index.html
    //app.UseSwaggerUI(s =>
    //{
    //    s.SwaggerEndpoint("/swagger/v1/swagger.json", "swagger v1");
    //    s.RoutePrefix = string.Empty;
    //});
    //Launch the app and navigate to https://localhost:{PORT}/scalar
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(ReactDevCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
