using Application.Results;

namespace Tests.ApplicationTests.Results;

public class ResultTests
{
    [Fact]
    public void WhenSuccess_Value_ShouldReturnValue()
    {
        //Arrange
        var result = Result<int, string>.Success(42);

        //Act
        var value = result.Value;

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, value);
    }

    [Fact]
    public void WhenSuccess_Error_ShouldThrow()
    {
        //Arrange
        var result = Result<int, string>.Success(42);

        //Act

        //Assert
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void WhenFailure_Error_ShouldReturnError()
    {
        //Arrange
        var result = Result<int, string>.Failure("failed");

        //Act
        var error = result.Error;

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("failed", error);
    }

    [Fact]
    public void WhenFailure_Value_ShouldThrow()
    {
        //Arrange
        var result = Result<int, string>.Failure("failed");

        //Act

        //Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
