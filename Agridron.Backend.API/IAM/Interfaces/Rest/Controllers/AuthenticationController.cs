using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Agridron.Backend.API.IAM.Application.CommandServices;
using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;
using Agridron.Backend.API.IAM.Interfaces.Rest.Transform;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Controllers;

/// <summary>REST endpoints for registration, email verification, sign-in, and sign-out.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Available Authentication Endpoints.")]
public class AuthenticationController(
    IUserCommandService userCommandService,
    ProblemDetailsFactory problemDetailsFactory) : ControllerBase
{
    /// <summary>Registers a new platform user and triggers verification email delivery.</summary>
    /// <returns>The created user resource.</returns>
    [AllowAnonymous]
    [HttpPost("sign-up")]
    [SwaggerOperation(
        Summary = "Sign up a new user",
        Description = "Creates a new platform user account with hashed password, generates an email verification token, and sends a verification email when Brevo is enabled.",
        OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status201Created, "User registered successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid registration payload")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A user with the given email already exists")]
    public async Task<IActionResult> SignUpAsync(
        [FromBody] SignUpResource resource,
        CancellationToken cancellationToken)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await userCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var userResource = UserAssembler.ToResourceFrom(result.Value!);
        return CreatedAtRoute("GetUserById", new { id = userResource.Id }, userResource);
    }

    /// <summary>Verifies a user's email address using a single-use verification token.</summary>
    /// <param name="redirectUrl">Optional absolute HTTP/HTTPS redirect target on success.</param>
    /// <returns>The verification confirmation resource or a redirect response.</returns>
    [AllowAnonymous]
    [HttpGet("verify-email")]
    [SwaggerOperation(
        Summary = "Verify user email address",
        Description = "Validates the single-use email verification token, marks the user's email as verified, and clears the token.",
        OperationId = "VerifyEmail")]
    [SwaggerResponse(StatusCodes.Status200OK, "Email verified successfully", typeof(EmailVerificationResource))]
    [SwaggerResponse(StatusCodes.Status302Found, "Email verified and redirected to frontend URL")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid or already consumed verification token")]
    public async Task<IActionResult> VerifyEmailAsync(
        [FromQuery] string? token,
        [FromQuery] string? redirectUrl,
        CancellationToken cancellationToken)
    {
        var result = await userCommandService.Handle(new VerifyEmailCommand(token ?? string.Empty), cancellationToken);
        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        if (!string.IsNullOrWhiteSpace(redirectUrl)
            && Uri.TryCreate(redirectUrl.Trim(), UriKind.Absolute, out var redirectUri)
            && (redirectUri.Scheme == Uri.UriSchemeHttp || redirectUri.Scheme == Uri.UriSchemeHttps))
        {
            return Redirect(redirectUri.ToString());
        }

        var user = result.Value!;
        return Ok(new EmailVerificationResource(
            user.Id.Value,
            user.Email.Value,
            user.EmailVerified,
            "Email verified successfully."));
    }

    /// <summary>Authenticates a user and issues a signed JWT.</summary>
    /// <returns>The authenticated user resource including the JWT.</returns>
    [AllowAnonymous]
    [HttpPost("sign-in")]
    [SwaggerOperation(
        Summary = "Sign in an existing user",
        Description = "Validates user credentials and issues a JWT Bearer token.",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "User authenticated successfully", typeof(AuthenticatedUserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid sign-in payload")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid email or password")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "User account is inactive")]
    public async Task<IActionResult> SignInAsync(
        [FromBody] SignInResource resource,
        CancellationToken cancellationToken)
    {
        var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await userCommandService.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return problemDetailsFactory.CreateProblemDetails(this, result.Error!);
        }

        var (user, token) = result.Value;
        return Ok(AuthenticatedUserAssembler.ToResourceFrom(user, token));
    }

    /// <summary>Signs out the authenticated user and records a sign-out event.</summary>
    [Authorize]
    [HttpPost("sign-out")]
    [SwaggerOperation(
        Summary = "Sign out current user",
        Description = "Records sign-out for the currently authenticated user.",
        OperationId = "SignOut")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User signed out successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Authentication required")]
    public async Task<IActionResult> SignOutAsync(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return problemDetailsFactory.CreateProblemDetails(this, IamErrors.InvalidCredentials);
        }

        var result = await userCommandService.Handle(new SignOutCommand(userId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : problemDetailsFactory.CreateProblemDetails(this, result.Error!);
    }

    private bool TryGetAuthenticatedUserId(out long userId)
    {
        var rawId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(rawId, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId)
               && userId > 0;
    }
}
