using Agridron.Backend.API.Shared.Domain.Model;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Http;

namespace Agridron.Backend.API.Tests.Shared.Interfaces.Rest.ProblemDetails;

public class ErrorProblemDetailsMapperTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.BusinessRule, 422)]
    [InlineData(ErrorType.Unexpected, 500)]
    public void ToStatusCode_ShouldMapEveryErrorType(ErrorType type, int expectedStatus)
    {
        // Arrange
        var error = new Error("SOME_CODE", "Some message.", type);

        // Act
        var status = error.ToStatusCode();

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Fact]
    public void ToProblemDetails_ShouldReturnRfc7807Payload()
    {
        // Arrange
        var error = Error.NotFound("FarmNotFound", "Farm with id 42 was not found.");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/farms/42";

        // Act
        var problemDetails = error.ToProblemDetails(context);

        // Assert
        problemDetails.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Resource Not Found");
        problemDetails.Detail.Should().Be("Farm with id 42 was not found.");
        problemDetails.Instance.Should().Be("/api/v1/farms/42");
        problemDetails.Type.Should().Be("https://httpstatuses.com/404");
        problemDetails.Extensions["code"].Should().Be("FarmNotFound");
        problemDetails.Extensions.Should().NotContainKey("details");
    }

    [Fact]
    public void ToProblemDetails_ShouldIncludeDetails_WhenProvided()
    {
        // Arrange
        var error = Error.Validation("ParcelGeometryInvalid", "Invalid polygon.", "Ring is not closed.");
        var context = new DefaultHttpContext();

        // Act
        var problemDetails = error.ToProblemDetails(context);

        // Assert
        problemDetails.Extensions["details"].Should().Be("Ring is not closed.");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapUnauthorizedErrorTo401()
    {
        // Arrange
        var error = Error.Unauthorized("InvalidCredentials", "Invalid email or password.");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/authentication/sign-in";

        // Act
        var problemDetails = error.ToProblemDetails(context);

        // Assert
        problemDetails.Status.Should().Be(401);
        problemDetails.Title.Should().Be("Unauthorized");
        problemDetails.Detail.Should().Be("Invalid email or password.");
        problemDetails.Instance.Should().Be("/api/v1/authentication/sign-in");
        problemDetails.Type.Should().Be("https://httpstatuses.com/401");
        problemDetails.Extensions["code"].Should().Be("InvalidCredentials");
    }

    [Fact]
    public void ToProblemDetails_ShouldMapForbiddenErrorTo403()
    {
        // Arrange
        var error = Error.Forbidden("InsufficientPermissions", "You do not have permission to perform this action.");
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/farms";

        // Act
        var problemDetails = error.ToProblemDetails(context);

        // Assert
        problemDetails.Status.Should().Be(403);
        problemDetails.Title.Should().Be("Forbidden");
        problemDetails.Detail.Should().Be("You do not have permission to perform this action.");
        problemDetails.Instance.Should().Be("/api/v1/farms");
        problemDetails.Type.Should().Be("https://httpstatuses.com/403");
        problemDetails.Extensions["code"].Should().Be("InsufficientPermissions");
    }
}
