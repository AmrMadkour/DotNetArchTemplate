using API.Middleware;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
