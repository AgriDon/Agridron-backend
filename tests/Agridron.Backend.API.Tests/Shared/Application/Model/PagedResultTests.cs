using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.Tests.Shared.Application.Model;

public class PagedResultTests
{
    [Fact]
    public void Empty_ShouldHaveNoItemsAndNoNextPage()
    {
        // Arrange
        var parameters = PaginationParameters.Create(0, 20);

        // Act
        var page = PagedResult<string>.Empty(parameters);

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
        page.TotalPages.Should().Be(0);
        page.HasNextPage.Should().BeFalse();
        page.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldComputeTotalPages()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).Select(i => $"item-{i}").ToList();
        var parameters = PaginationParameters.Create(0, 20);

        // Act
        var page = PagedResult<string>.Create(items, 55, parameters);

        // Assert
        page.TotalPages.Should().Be(3);
        page.HasNextPage.Should().BeTrue();
        page.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldReportPreviousPage_WhenPageIsGreaterThanZero()
    {
        // Arrange
        var parameters = PaginationParameters.Create(1, 20);

        // Act
        var page = PagedResult<string>.Create([], 55, parameters);

        // Assert
        page.HasPreviousPage.Should().BeTrue();
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrow_WhenTotalCountIsNegative()
    {
        // Arrange
        var parameters = PaginationParameters.Create(0, 20);

        // Act
        var action = () => PagedResult<string>.Create([], -1, parameters);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Map_ShouldProjectItemsAndPreserveMetadata()
    {
        // Arrange
        var parameters = PaginationParameters.Create(0, 2);
        var page = PagedResult<int>.Create([1, 2], 10, parameters);

        // Act
        var mapped = page.Map(value => value * 10);

        // Assert
        mapped.Items.Should().Equal(10, 20);
        mapped.TotalCount.Should().Be(10);
        mapped.TotalPages.Should().Be(5);
    }

    [Fact]
    public void TotalPages_ShouldBeOne_WhenQueryIsUnpaged()
    {
        // Arrange
        var parameters = PaginationParameters.Unpaged;

        // Act
        var page = PagedResult<int>.Create([1, 2, 3], 3, parameters);

        // Assert
        page.Size.Should().Be(0);
        page.TotalPages.Should().Be(1);
        page.HasNextPage.Should().BeFalse();
    }
}
