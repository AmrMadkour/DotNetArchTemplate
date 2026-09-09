using System.Net;
using API.Controllers;
using API.Dtos;
using Application.Results;
using Application.Services;
using Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        var ordersController = new OrdersController(quoteServiceMock.Object, CreateHttpClientFactoryMock().Object)
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
        var ordersController = new OrdersController(quoteServiceMock.Object, CreateHttpClientFactoryMock().Object);

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
}
