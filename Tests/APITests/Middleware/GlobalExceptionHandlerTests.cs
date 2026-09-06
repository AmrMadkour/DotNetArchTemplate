using API.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.APITests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenCalled_ShouldSetResponseStatusCodeToInternalServerError()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock.Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>())).ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();

        //Act
        await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        //Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WhenCalled_ShouldWriteProblemDetailsWithExpectedFields()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        ProblemDetails? capturedProblemDetails = null;
        problemDetailsServiceMock
            .Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .Callback<ProblemDetailsContext>(context => capturedProblemDetails = context.ProblemDetails)
            .ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();

        //Act
        await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        //Assert
        Assert.NotNull(capturedProblemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, capturedProblemDetails!.Status);
        Assert.Equal("An unexpected error occurred.", capturedProblemDetails.Title);
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.6.1", capturedProblemDetails.Type);
    }

    [Fact]
    public async Task TryHandleAsync_WhenProblemDetailsServiceWritesSuccessfully_ShouldReturnTrue()
    {
        //Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var problemDetailsServiceMock = new Mock<IProblemDetailsService>();
        problemDetailsServiceMock.Setup(p => p.TryWriteAsync(It.IsAny<ProblemDetailsContext>())).ReturnsAsync(true);
        var handler = new GlobalExceptionHandler(loggerMock.Object, problemDetailsServiceMock.Object);
        var httpContext = new DefaultHttpContext();

        //Act
        var result = await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        //Assert
        Assert.True(result);
    }
}
