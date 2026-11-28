using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Agridron.Backend.API.IAM.Application.CommandServices;
using Agridron.Backend.API.IAM.Application.QueryServices;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;
using Agridron.Backend.API.IAM.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Controllers;

/// <summary>
///     HTTP REST controller exposing authenticated user profile endpoints (AU-005).
/// </summary>
/// <param name="userCommandService">Application command service handling profile updates.</param>
/// <param name="userQueryService">Application query service handling user lookups.</param>
/// <param name="problemDetailsFactory">Factory translating domain errors into RFC 7807 Problem Details.</param>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available User Endpoints.")]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>
    ///     Retrieves a user profile by its database identifier.
    /// </summary>
    /// <param name="id">The target user identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>200 OK</c> with <see cref="UserResource" />, or <c>404 Not Found</c>.</returns>
    [HttpGet("{id:long}", Name = "GetUserById")]
    [SwaggerOperation(
        Summary = "Get user profile by ID",
        Description = "Retrieves the profile details of a registered user.",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "User found successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication required")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> GetUserByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(UserAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    /// <summary>
    ///     Updates basic profile information for a user. Allowed only for the account owner or an <c>Admin</c>.
    /// </summary>
    /// <param name="id">The target user identifier.</param>
    /// <param name="resource">The profile update payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>200 OK</c> with the updated <see cref="UserResource" />, <c>403 Forbidden</c>, or <c>404 Not Found</c>.</returns>
    [HttpPatch("{id:long}")]
    [SwaggerOperation(
        Summary = "Update user profile",
        Description = "Updates basic profile fields (full name and phone number). Only the user themselves or an Admin can perform this action.",
        OperationId = "UpdateUserProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "User profile updated successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid profile payload")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication required")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Insufficient permissions to modify this user")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UpdateUserProfileAsync(
        long id,
        [FromBody] UpdateUserResource resource,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
        {
            return problemDetailsFactory.CreateProblemDetails(this, IamErrors.InvalidCredentials);
        }

        if (authenticatedUserId != id && !IsAdmin())
        {
            return problemDetailsFactory.CreateProblemDetails(this, IamErrors.ForbiddenProfileAccess(id));
        }

        var command = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await userCommandService.Handle(command, cancellationToken);

        return result.IsSuccess
            ? Ok(UserAssembler.ToResourceFrom(result.Value!))
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    private bool TryGetAuthenticatedUserId(out long userId)
    {
        var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(rawId, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId)
               && userId > 0;
    }

    private bool IsAdmin()
    {
        const string adminRole = nameof(UserRole.Admin);

        return User.IsInRole(adminRole)
               || User.Claims.Any(claim =>
                   (claim.Type == ClaimTypes.Role || claim.Type == "role")
                   && string.Equals(claim.Value, adminRole, StringComparison.OrdinalIgnoreCase));
    }
}
