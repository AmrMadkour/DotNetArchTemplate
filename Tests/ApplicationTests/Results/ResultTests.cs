using Application.Results;

namespace Tests.ApplicationTests.Results;

public class ResultTests
{
    [Fact]
    public void WhenSuccess_Value_ShouldReturnValue()
    {
        //Arrange
        var result = Result<int>.Success(42);

        //Act
        var value = result.Value;

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, value);
    }

    [Fact]
    public void WhenSuccess_ErrorMessage_ShouldThrow()
    {
        //Arrange
        var result = Result<int>.Success(42);

        //Act

        //Assert
        Assert.Throws<InvalidOperationException>(() => result.ErrorMessage);
    }

    [Fact]
    public void WhenFailure_ErrorMessage_ShouldReturnErrorMessage()
    {
        //Arrange
        var result = Result<int>.Failure("failed");

        //Act
        var errorMessage = result.ErrorMessage;

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("failed", errorMessage);
    }

    [Fact]
    public void WhenFailure_Value_ShouldThrow()
    {
        //Arrange
        var result = Result<int>.Failure("failed");

        //Act

        //Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenFailureAndNoErrorMessage_ErrorMessage_ShouldThrow(string? errorMessage)
    {
        //Arrange

        //Act

        //Assert
        Assert.Throws<ArgumentException>(() => Result<int>.Failure(errorMessage!));
    }
}
