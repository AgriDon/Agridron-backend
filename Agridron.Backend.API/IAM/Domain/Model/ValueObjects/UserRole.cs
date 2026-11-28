using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Roles supported by the IAM bounded context.
/// </summary>
public enum UserRole
{
    /// <summary>Agricultural producer / farm owner (default role).</summary>
    Farmer = 0,

    /// <summary>Drone flight operator.</summary>
    Operator = 1,

    /// <summary>Platform administrator.</summary>
    Admin = 2
}

/// <summary>
///     Factory extensions for parsing <see cref="UserRole" /> values from strings.
/// </summary>
/// <remarks>
///     Design Pattern: Factory Method — centralizes safe role parsing and default-role resolution
///     without leaking enum parsing details into application services.
/// </remarks>
public static class UserRoleExtensions
{
    extension(UserRole)
    {
        /// <summary>
        ///     Resolves a <see cref="UserRole" /> from its textual name, defaulting to <see cref="UserRole.Farmer" />
        ///     when the input is null or blank.
        /// </summary>
        /// <param name="roleName">The optional textual role name (case-insensitive).</param>
        /// <returns>The resolved <see cref="UserRole" /> value.</returns>
        /// <exception cref="DomainException">Thrown when <paramref name="roleName" /> does not match any defined role.</exception>
        public static UserRole FromName(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return UserRole.Farmer;
            }

            if (Enum.TryParse<UserRole>(roleName.Trim(), ignoreCase: true, out var parsed)
                && Enum.IsDefined(parsed))
            {
                return parsed;
            }

            throw new DomainException(IamErrors.InvalidRole(roleName));
        }
    }
}
