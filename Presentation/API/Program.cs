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
// {Properties} renders BeginScope's TraceId/CorrelationId (and anything else pushed into scope) —
// without it Serilog's default template drops them entirely, unlike NLog's ${all-event-properties} in MinimalAPI's nlog.config.
const string outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}";
builder.Host.UseSerilog((_, configuration) => configuration
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate));

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

// Points at MinimalAPI, treated here as if it were a separate service — used only by
// OrdersController's manual-verification call to MinimalAPI's LoadLogsForTest endpoint.
builder.Services.AddHttpClient("MinimalApi", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:MinimalApiBaseUrl"]!));

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

// Skipped in Development: the React dev server and Swagger both target plain http here,
// and redirecting the CORS preflight OPTIONS request to https breaks it (browsers won't follow a redirect on preflight).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(ReactDevCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
