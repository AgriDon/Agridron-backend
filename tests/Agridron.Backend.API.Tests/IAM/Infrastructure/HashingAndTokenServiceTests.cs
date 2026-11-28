using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Infrastructure.Email.Brevo.Services;
using Agridron.Backend.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using Agridron.Backend.API.IAM.Infrastructure.Tokens.JWT.Services;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Agridron.Backend.API.Tests.IAM.Infrastructure;

public class HashingAndTokenServiceTests
{
    private const string TestJwtSecret = "agridron-test-signing-key-with-at-least-32-bytes!";

    [Fact]
    public void HashingService_ShouldProduceSaltedHashAndVerifyCorrectPassword()
    {
        // Arrange
        var hashingService = new HashingService();
        const string rawPassword = "MySuperSecretPassword!2026";

        // Act
        var hash1 = hashingService.HashPassword(rawPassword);
        var hash2 = hashingService.HashPassword(rawPassword);

        // Assert
        hash1.Should().NotBe(rawPassword);
        hash1.Should().NotBe(hash2);
        hashingService.VerifyPassword(rawPassword, hash1).Should().BeTrue();
        hashingService.VerifyPassword("WrongPassword", hash1).Should().BeFalse();
        hashingService.VerifyPassword(rawPassword, "not-a-bcrypt-hash").Should().BeFalse();
    }

    [Fact]
    public void TokenService_ShouldGenerateValidJwtWithExpectedClaims()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = TestJwtSecret,
                ["Jwt:Issuer"] = "Agridron.Backend.API",
                ["Jwt:Audience"] = "Agridron.Frontend",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var tokenService = new TokenService(configuration);
        var user = new User("Samir Perez", EmailAddress.Create("samir@agridron.pe"), "$2a$11$hash", UserRole.Admin);
        SetUserId(user, 42);

        // Act
        var jwt = tokenService.GenerateToken(user);

        // Assert
        jwt.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Agridron.Backend.API",
            ValidateAudience = true,
            ValidAudience = "Agridron.Frontend",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret))
        };

        var principal = handler.ValidateToken(jwt, validationParameters, out var validatedToken);
        validatedToken.Should().BeOfType<JwtSecurityToken>();

        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be("42");
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.Should().Be("samir@agridron.pe");
        principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value.Should().Be("Samir Perez");
        principal.FindFirst("role")?.Value.Should().Be("Admin");
    }

    [Fact]
    public void TokenService_WhenSecretIsMissingOrUnresolvedPlaceholder_ShouldThrowDomainException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "${UNSET_AGRIDRON_JWT_SECRET_TEST_VAR}",
                ["Jwt:Issuer"] = "Agridron.Backend.API",
                ["Jwt:Audience"] = "Agridron.Frontend"
            })
            .Build();

        var tokenService = new TokenService(configuration);
        var user = new User("Samir Perez", EmailAddress.Create("samir@agridron.pe"), "$2a$11$hash");
        SetUserId(user, 1);

        // Act
        var act = () => tokenService.GenerateToken(user);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be("JwtSecretNotConfigured");
    }

    [Fact]
    public void BrevoEmailService_BuildWelcomeEmailHtml_ShouldContainBrandingRoleAndVerificationButton()
    {
        // Arrange
        const string verificationUrl = "http://localhost:5173/verify-email?token=abc123token";

        // Act
        var html = BrevoEmailService.BuildWelcomeEmailHtml("Samir <Farmer>", "Farmer", verificationUrl);

        // Assert
        html.Should().Contain("AgriDron Solutions");
        html.Should().Contain("Samir &lt;Farmer&gt;");
        html.Should().Contain("Agricultor / Farmer");
        html.Should().Contain("Verificar mi correo electrónico");
        html.Should().Contain(verificationUrl);
        html.Should().Contain("#16a34a");
    }

    private static void SetUserId(User user, long id)
    {
        var property = typeof(User).BaseType!.BaseType!.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(user, new UserId(id));
    }
}
