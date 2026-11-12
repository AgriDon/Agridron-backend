using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.Tests.Shared.Application.Model;

public class PaginationParametersTests
{
    [Fact]
    public void Create_ShouldReturnUnpaged_WhenNoParametersAreProvided()
    {
        // Act
        var parameters = PaginationParameters.Create(null, null);

        // Assert
        parameters.IsPaged.Should().BeFalse();
        parameters.Page.Should().Be(0);
        parameters.Size.Should().Be(0);
        parameters.Skip.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldUseDefaults_WhenOnlyPageIsProvided()
    {
        // Act
        var parameters = PaginationParameters.Create(2, null);

        // Assert
        parameters.IsPaged.Should().BeTrue();
        parameters.Page.Should().Be(2);
        parameters.Size.Should().Be(PaginationParameters.DefaultSize);
        parameters.Skip.Should().Be(2 * PaginationParameters.DefaultSize);
    }

    [Fact]
    public void Create_ShouldClampSize_WhenSizeExceedsMaximum()
    {
        // Act
        var parameters = PaginationParameters.Create(0, 500);

        // Assert
        parameters.Size.Should().Be(PaginationParameters.MaxSize);
    }

    [Fact]
    public void Create_ShouldClampSize_WhenSizeIsZeroOrNegative()
    {
        // Act
        var zero = PaginationParameters.Create(0, 0);
        var negative = PaginationParameters.Create(0, -10);

        // Assert
        zero.Size.Should().Be(1);
        negative.Size.Should().Be(1);
    }

    [Fact]
    public void Create_ShouldClampPage_WhenPageIsNegative()
    {
        // Act
        var parameters = PaginationParameters.Create(-5, 10);

        // Assert
        parameters.Page.Should().Be(0);
        parameters.Skip.Should().Be(0);
    }
}
