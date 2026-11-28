using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>Authorization roles supported by the platform.</summary>
public enum UserRole
{
    Farmer = 0,
    Operator = 1,
    Admin = 2
}

public static class UserRoleExtensions
{
    extension(UserRole)
    {
        /// <summary>Parses a role name case-insensitively, defaulting to <see cref="UserRole.Farmer" /> when blank.</summary>
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
