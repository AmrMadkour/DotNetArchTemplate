using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MinimalAPI.Middleware;
using Moq;

namespace Tests.MinimalAPITests.Middleware;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenCalled_ShouldCallNextDelegate()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new RequestLoggingMiddleware(next, loggerMock.Object);
        var httpContext = new DefaultHttpContext();

        //Act
        await middleware.InvokeAsync(httpContext);

        //Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextDelegateThrows_ShouldPropagateException()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
        RequestDelegate next = _ => throw new InvalidOperationException("boom");
        var middleware = new RequestLoggingMiddleware(next, loggerMock.Object);
        var httpContext = new DefaultHttpContext();

        //Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(httpContext));
    }
}
