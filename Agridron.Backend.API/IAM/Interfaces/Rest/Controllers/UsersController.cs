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

/// <summary>REST endpoints for authenticated user profile lookup and updates.</summary>
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
    /// <summary>Retrieves a user profile by identifier.</summary>
    /// <returns>The matching user resource.</returns>
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

    /// <summary>Updates basic profile fields for the account owner or an administrator.</summary>
    /// <returns>The updated user resource.</returns>
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
