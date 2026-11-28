using System.Net;
using System.Net.Http.Json;
using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using Agridron.Backend.API.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agridron.Backend.API.IAM.Infrastructure.Email.Brevo.Services;

/// <summary>
///     Native <see cref="IHttpClientFactory" /> adapter for sending transactional verification emails via Brevo REST API v3.
/// </summary>
/// <remarks>
///     Design Pattern: Strategy — concrete transactional email delivery strategy consuming Brevo's
///     <c>POST /v3/smtp/email</c> endpoint without introducing external third-party SDK dependencies.
/// </remarks>
internal sealed class BrevoEmailService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<BrevoEmailService> logger) : IBrevoEmailService
{
    /// <summary>Named <see cref="HttpClient" /> identifier registered in DI.</summary>
    public const string HttpClientName = "Brevo";

    private const string DefaultFrontendBaseUrl = "http://localhost:5173";

    /// <inheritdoc />
    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string role,
        string verificationToken,
        CancellationToken cancellationToken = default)
    {
        var brevoSection = configuration.GetSection("Brevo");
        var enabled = bool.TryParse(brevoSection["Enabled"], out var parsedEnabled) && parsedEnabled;
        if (!enabled)
        {
            logger.LogInformation("Brevo email delivery is disabled. Skipping welcome verification email.");
            return;
        }

        var apiKey = TokenService.ResolveSecret(brevoSection["ApiKey"]);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Brevo is enabled but ApiKey is not configured. Skipping welcome verification email.");
            return;
        }

        var senderEmail = brevoSection["SenderEmail"];
        var senderName = brevoSection["SenderName"] ?? "AgriDron Solutions";
        if (string.IsNullOrWhiteSpace(senderEmail))
        {
            logger.LogWarning("Brevo SenderEmail is not configured. Skipping welcome verification email.");
            return;
        }

        var frontendBaseUrl = (brevoSection["FrontendBaseUrl"] ?? DefaultFrontendBaseUrl).Trim().TrimEnd('/');
        var verificationUrl = $"{frontendBaseUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";
        var htmlContent = BuildWelcomeEmailHtml(fullName, role, verificationUrl);

        var payload = new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email = toEmail, name = fullName } },
            subject = "Verifica tu correo electrónico — AgriDron Solutions",
            htmlContent
        };

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = JsonContent.Create(payload)
        };

        if (!client.DefaultRequestHeaders.Contains("api-key"))
        {
            request.Headers.Add("api-key", apiKey);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Brevo returned non-success status code {StatusCode} while sending welcome verification email.",
                (int)response.StatusCode);
            return;
        }

        logger.LogInformation("Welcome verification email accepted by Brevo (status {StatusCode}).", (int)response.StatusCode);
    }

    /// <summary>
    ///     Builds an email-client-compatible inline-styled HTML welcome and verification message.
    /// </summary>
    /// <param name="fullName">The user's display name.</param>
    /// <param name="role">The user's assigned role name.</param>
    /// <param name="verificationUrl">The absolute verification link containing the single-use token.</param>
    /// <returns>The rendered HTML document string.</returns>
    internal static string BuildWelcomeEmailHtml(string fullName, string role, string verificationUrl)
    {
        var encodedName = WebUtility.HtmlEncode(fullName);
        var encodedRoleLabel = WebUtility.HtmlEncode(FormatRoleLabel(role));
        var encodedUrl = WebUtility.HtmlEncode(verificationUrl);

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Verifica tu correo — AgriDron Solutions</title>
            </head>
            <body style="margin: 0; padding: 0; background-color: #f3f4f6; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; color: #1f2937;">
              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="background-color: #f3f4f6; padding: 32px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" cellpadding="0" cellspacing="0" width="600" style="max-width: 600px; width: 100%; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);">
                      <!-- Header -->
                      <tr>
                        <td style="background-color: #14532d; background: linear-gradient(135deg, #14532d 0%, #166534 100%); padding: 32px 36px; text-align: center;">
                          <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
                            <tr>
                              <td align="center">
                                <span style="display: inline-block; background-color: rgba(255, 255, 255, 0.14); color: #dcfce7; font-size: 12px; font-weight: 700; letter-spacing: 1.2px; text-transform: uppercase; padding: 6px 14px; border-radius: 999px;">
                                  Precisión Agrícola con Drones
                                </span>
                              </td>
                            </tr>
                            <tr>
                              <td align="center" style="padding-top: 14px;">
                                <h1 style="margin: 0; color: #ffffff; font-size: 26px; font-weight: 700; letter-spacing: -0.3px;">
                                  AgriDron Solutions
                                </h1>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding: 36px 36px 28px 36px;">
                          <h2 style="margin: 0 0 12px 0; color: #111827; font-size: 20px; font-weight: 700;">
                            ¡Hola, {encodedName}!
                          </h2>
                          <p style="margin: 0 0 18px 0; color: #374151; font-size: 15px; line-height: 1.6;">
                            Gracias por registrarte en <strong>AgriDron Solutions</strong>. Tu cuenta ha sido creada con el perfil:
                          </p>

                          <table role="presentation" cellpadding="0" cellspacing="0" style="margin: 0 0 22px 0;">
                            <tr>
                              <td style="background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; padding: 10px 16px;">
                                <span style="color: #166534; font-size: 13px; font-weight: 700;">
                                  Rol asignado: {encodedRoleLabel}
                                </span>
                              </td>
                            </tr>
                          </table>

                          <p style="margin: 0 0 28px 0; color: #374151; font-size: 15px; line-height: 1.6;">
                            Para confirmar la titularidad de tu correo electrónico y activar todas las funciones de gestión de fundos, parcelas y misiones de fumigación, haz clic en el siguiente botón:
                          </p>

                          <!-- Primary CTA Button -->
                          <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin: 0 0 28px 0;">
                            <tr>
                              <td align="center">
                                <a href="{encodedUrl}" target="_blank" rel="noopener noreferrer"
                                   style="display: inline-block; background-color: #16a34a; color: #ffffff; font-size: 15px; font-weight: 700; text-decoration: none; padding: 14px 32px; border-radius: 8px;">
                                  Verificar mi correo electrónico
                                </a>
                              </td>
                            </tr>
                          </table>

                          <!-- Plain-text fallback link -->
                          <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="background-color: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; padding: 16px; margin-bottom: 24px;">
                            <tr>
                              <td style="padding: 14px 16px;">
                                <p style="margin: 0 0 6px 0; color: #4b5563; font-size: 12px; font-weight: 600;">
                                  ¿El botón no funciona? Copia y pega este enlace en tu navegador:
                                </p>
                                <p style="margin: 0; word-break: break-all; font-size: 12px; line-height: 1.5;">
                                  <a href="{encodedUrl}" style="color: #15803d; text-decoration: underline;">{encodedUrl}</a>
                                </p>
                              </td>
                            </tr>
                          </table>

                          <!-- Security Notice -->
                          <p style="margin: 0; color: #6b7280; font-size: 13px; line-height: 1.6;">
                            <strong>Nota de seguridad:</strong> Este enlace es personal y de un solo uso. Si no creaste una cuenta en AgriDron Solutions, puedes ignorar este mensaje de forma segura.
                          </p>
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="background-color: #f9fafb; border-top: 1px solid #e5e7eb; padding: 20px 36px; text-align: center;">
                          <p style="margin: 0 0 4px 0; color: #374151; font-size: 13px; font-weight: 600;">
                            AgriDron Solutions
                          </p>
                          <p style="margin: 0; color: #9ca3af; font-size: 12px; line-height: 1.5;">
                            Plataforma inteligente de operaciones agrícolas con drones &bull; Mensaje automático, por favor no respondas a este correo.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string FormatRoleLabel(string role) =>
        role.Trim().ToUpperInvariant() switch
        {
            "FARMER" => "Agricultor / Farmer",
            "OPERATOR" => "Operador / Operator",
            "ADMIN" => "Administrador / Admin",
            _ => role
        };
}
