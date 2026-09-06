using Application;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using MinimalAPI.Endpoints;
using MinimalAPI.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Required for Swashbuckle to discover minimal API endpoints (no AddControllers() here to register it for us)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

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

app.MapOrderEndpoints();

app.Run();

