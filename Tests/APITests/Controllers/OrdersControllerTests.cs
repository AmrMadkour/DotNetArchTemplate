using System.Net;
using API.Controllers;
using API.Dtos;
using Application.Results;
using Application.Services;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Tests.Builders;

namespace Tests.APITests.Controllers;

public class OrdersControllerTests
{
    // Stands in for the real MinimalAPI call OrdersController makes after a successful quote —
    // always returns 200 without hitting the network, so the test stays isolated.
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private static Mock<IHttpClientFactory> CreateHttpClientFactoryMock()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient("MinimalApi"))
            .Returns(new HttpClient(new FakeHttpMessageHandler()) { BaseAddress = new Uri("http://localhost") });
        return httpClientFactoryMock;
    }

    [Fact]
    public async Task Quote_WhenPrepareQuoteSucceeds_ShouldReturnOk()
    {
        //Arrange
        var expectedQuote = new QuoteDtoBuilder().Build();

        var quoteServiceMock = new Mock<IQuoteService>();
        quoteServiceMock.Setup(q => q.PrepareQuote(It.IsAny<Order>())).ReturnsAsync(Result<Quote>.Success(new QuoteBuilder().Build()));
        var ordersController = new OrdersController(
            quoteServiceMock.Object,
            CreateHttpClientFactoryMock().Object,
            TimeProvider.System,
            new Mock<ILogger<OrdersController>>().Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        //Act
        var response = await ordersController.Quote(new OrderDtoBuilder().Build());

        //Assert
        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.IsType<QuoteDto>(okResult.Value);
        Assert.Equivalent(expectedQuote, okResult.Value);

        //var okResultFa = response.Result.Should().BeOfType<OkObjectResult>().Which;
        //okResultFa.StatusCode.Should().Be(StatusCodes.Status200OK);
        //okResultFa.Value.Should().BeOfType<QuoteDto>().And.BeEquivalentTo(expectedQuote);
    }

    [Fact]
    public async Task Quote_WhenPrepareQuoteFails_ShouldReturnBadRequest()
    {
        //Arrange
        var expectedError = "Failed to prepare quote";

        var quoteServiceMock = new Mock<IQuoteService>();
        quoteServiceMock.Setup(q => q.PrepareQuote(It.IsAny<Order>())).ReturnsAsync(Result<Quote>.Failure(expectedError));
        var ordersController = new OrdersController(
            quoteServiceMock.Object,
            CreateHttpClientFactoryMock().Object,
            TimeProvider.System,
            new Mock<ILogger<OrdersController>>().Object);

        //Act
        var response = await ordersController.Quote(new OrderDtoBuilder().Build());

        //Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.IsType<string>(badRequestResult.Value);
        Assert.Equivalent(expectedError, badRequestResult.Value);

        //var badRequestResultFa = response.Result.Should().BeOfType<BadRequestObjectResult>().Which;
        //badRequestResultFa.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        //badRequestResultFa.Value.Should().BeOfType<string>().And.BeEquivalentTo(expectedError);
    }

    // Proves the point of injecting TimeProvider: a FakeTimeProvider set to a known instant drives
    // what DemoTimeProviderVsDateTimeNow logs — something DateTime.UtcNow could never be made to do
    // deterministically in a test.
    [Fact]
    public async Task Quote_WhenPrepareQuoteSucceeds_ShouldLogInjectedTimeProviderNow()
    {
        //Arrange
        var quoteServiceMock = new Mock<IQuoteService>();
        quoteServiceMock.Setup(q => q.PrepareQuote(It.IsAny<Order>())).ReturnsAsync(Result<Quote>.Success(new QuoteBuilder().Build()));
        var fixedNow = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FakeTimeProvider(fixedNow);
        var loggerMock = new Mock<ILogger<OrdersController>>();
        var ordersController = new OrdersController(
            quoteServiceMock.Object,
            CreateHttpClientFactoryMock().Object,
            fakeTimeProvider,
            loggerMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        //Act
        await ordersController.Quote(new OrderDtoBuilder().Build());

        //Assert — checks the structured "ProviderNow" value directly, rather than the formatted message
        //string, so the assertion doesn't depend on the logger's culture-dependent ToString() format.
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => ((IReadOnlyList<KeyValuePair<string, object>>)state)
                    .Any(kv => kv.Key == "ProviderNow" && kv.Value.Equals(fixedNow))),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
