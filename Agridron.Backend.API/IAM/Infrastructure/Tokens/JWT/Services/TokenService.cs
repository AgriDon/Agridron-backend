using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Agridron.Backend.API.IAM.Infrastructure.Tokens.JWT.Services;

internal sealed class TokenService(IConfiguration configuration) : ITokenService
{
    private const int MinSecretLengthBytes = 32;
    private const int DefaultExpirationMinutes = 1440;

    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var jwtSection = configuration.GetSection("Jwt");
        var secret = ResolveSecret(jwtSection["Secret"]);
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < MinSecretLengthBytes)
        {
            throw new DomainException(IamErrors.JwtSecretNotConfigured);
        }

        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], CultureInfo.InvariantCulture, out var parsed)
                                && parsed > 0
            ? parsed
            : DefaultExpirationMinutes;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new("role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
            audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    internal static string? ResolveSecret(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var trimmed = rawValue.Trim();
        if (trimmed.StartsWith("${", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            var envVarName = trimmed[2..^1].Trim();
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            return string.IsNullOrWhiteSpace(envValue) ? null : envValue;
        }

        var expanded = Environment.ExpandEnvironmentVariables(trimmed);
        return string.IsNullOrWhiteSpace(expanded) ? null : expanded;
    }
}
