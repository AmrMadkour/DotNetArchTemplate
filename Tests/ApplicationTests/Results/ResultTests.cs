using Application.Results;
using Domain.Models;
using FluentAssertions;
using Moq;
using Xunit.Sdk;

namespace Tests.ApplicationTests.Results;

public class ResultTests
{
    [Fact]
    public void Success_WhenValueIsNull_ShouldThrow()
    {
        //Arrange

        //Act

        //Assert
        Assert.Throws<ArgumentNullException>(() => Result<string?>.Success(null));

        //Action act = () => Result<string?>.Success(null);
        //act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Success_WhenHasValue_ShouldReturnValueAndNoAccessToErrorMessage()
    {
        //Arrange
        var expectedError = "Cannot access ErrorMessage on a successful Result.";
        var result = Result<int>.Success(42);

        //Act

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        var ex = Assert.Throws<InvalidOperationException>(() => result.ErrorMessage);
        Assert.Equal(expectedError, ex.Message);

        //result.Should().BeOfType<Result<int>>()
        //    .And.Match<Result<int>>(r => r.IsSuccess)
        //    .And.Match<Result<int>>(r => r.Value == 42);
        //Action act = () => { var errorMessage = result.ErrorMessage; };
        //act.Should().Throw<InvalidOperationException>().Which.Message.Should().Be(expectedError);
    }

    [Fact]
    public void Failure_WhenErrorMessageIsNull_ShouldThrow()
    {
        //Arrange

        //Act

        //Assert
        Assert.Throws<ArgumentNullException>(() => Result<string?>.Failure(null!));

        //Action act = () => Result<string?>.Failure(null!);
        //act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Failure_WhenErrorMessageIsEmpty_ShouldThrow()
    {
        //Arrange

        //Act

        //Assert
        Assert.Throws<ArgumentException>(() => Result<string?>.Failure(string.Empty));

        //Action act = () => Result<string?>.Failure(string.Empty);
        //act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_WhenHasErrorMessage_ShouldReturnErrorMessageAndNoAccessToValue()
    {
        //Arrange
        var expectedError = "Cannot access Value on a failed Result.";
        var expectedErrorMessage = "failed";
        var result = Result<string?>.Failure("failed");

        //Act

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorMessage, result.ErrorMessage);
        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Equal(expectedError, ex.Message);

        //result.Should().BeOfType<Result<string?>>()
        //    .And.Match<Result<string?>>(r => !r.IsSuccess)
        //    .And.Match<Result<string?>>(r => r.ErrorMessage == expectedErrorMessage);
        //Action act = () => { var value = result.Value; };
        //act.Should().Throw<InvalidOperationException>().Which.Message.Should().Be(expectedError);
    }
}
