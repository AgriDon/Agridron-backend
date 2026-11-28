using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using Agridron.Backend.API.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agridron.Backend.API.IAM.Infrastructure.Email.Brevo.Services;

/// <summary>HTTP client adapter for Brevo transactional email delivery.</summary>
internal sealed class BrevoEmailService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment,
    ILogger<BrevoEmailService> logger) : IBrevoEmailService
{
    public const string HttpClientName = "Brevo";
    public const string LogoFileName = "agridron-logo.png";

    private const string DefaultFrontendBaseUrl = "http://localhost:5173";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
        var logoBase64 = await TryLoadLogoBase64Async(cancellationToken);
        var hasInlineLogo = !string.IsNullOrWhiteSpace(logoBase64);
        var htmlContent = BuildWelcomeEmailHtml(fullName, role, verificationUrl, hasInlineLogo);

        var payload = new BrevoEmailPayload(
            new BrevoEmailParty(senderName, senderEmail),
            [new BrevoEmailParty(fullName, toEmail)],
            "Confirma tu correo electrónico — AgriDron Solutions",
            htmlContent,
            hasInlineLogo ? [new BrevoAttachment(LogoFileName, logoBase64!)] : null);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
        {
            Content = JsonContent.Create(payload, options: SerializerOptions)
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

    internal static string BuildWelcomeEmailHtml(
        string fullName,
        string role,
        string verificationUrl,
        bool includeInlineLogo = true)
    {
        var encodedName = WebUtility.HtmlEncode(fullName);
        var encodedUrl = WebUtility.HtmlEncode(verificationUrl);
        var introParagraph = BuildRoleIntroText(encodedName, role);

        var logoCell = includeInlineLogo
            ? $"""
               <td width="56" valign="middle" style="padding-right: 14px;">
                 <img src="cid:{LogoFileName}" alt="AgriDron Solutions" width="56" height="56" style="display: block; width: 56px; height: 56px; border: 0; border-radius: 50%;" />
               </td>
               """
            : string.Empty;

        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Confirma tu correo electrónico — AgriDron Solutions</title>
            </head>
            <body style="margin: 0; padding: 0; background-color: #f4f5f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #111827; -webkit-font-smoothing: antialiased;">
              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="background-color: #f4f5f7; padding: 40px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" cellpadding="0" cellspacing="0" width="540" style="max-width: 540px; width: 100%; background-color: #ffffff; border: 1px solid #e5e7eb; border-radius: 12px;">
                      <tr>
                        <td style="padding: 28px 36px 22px 36px; border-bottom: 1px solid #f0f1f3;">
                          <table role="presentation" cellpadding="0" cellspacing="0">
                            <tr>
                              {logoCell}
                              <td valign="middle">
                                <span style="font-size: 17px; font-weight: 700; color: #111827; letter-spacing: -0.2px;">
                                  AgriDron Solutions
                                </span>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>

                      <tr>
                        <td style="padding: 32px 36px 36px 36px;">
                          <h1 style="margin: 0 0 16px 0; font-size: 20px; font-weight: 600; color: #111827; line-height: 1.35; letter-spacing: -0.2px;">
                            Confirma tu correo electrónico
                          </h1>
                          <p style="margin: 0 0 28px 0; font-size: 14px; color: #374151; line-height: 1.65;">
                            {introParagraph}
                          </p>

                          <table role="presentation" cellpadding="0" cellspacing="0" style="margin: 0 0 28px 0;">
                            <tr>
                              <td align="left">
                                <a href="{encodedUrl}" target="_blank" rel="noopener noreferrer"
                                   style="display: inline-block; background-color: #15803d; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; padding: 13px 28px; border-radius: 8px; line-height: 1.2;">
                                  Confirmar correo electrónico
                                </a>
                              </td>
                            </tr>
                          </table>

                          <p style="margin: 0 0 6px 0; font-size: 12px; color: #6b7280; line-height: 1.55;">
                            Si tienes problemas con el botón, copia y pega este enlace en tu navegador:
                          </p>
                          <p style="margin: 0; font-size: 12px; line-height: 1.55; word-break: break-all;">
                            <a href="{encodedUrl}" style="color: #15803d; text-decoration: underline;">{encodedUrl}</a>
                          </p>
                        </td>
                      </tr>
                    </table>

                    <table role="presentation" cellpadding="0" cellspacing="0" width="540" style="max-width: 540px; width: 100%;">
                      <tr>
                        <td align="center" style="padding: 20px 16px 0 16px;">
                          <p style="margin: 0; font-size: 11px; color: #9ca3af; line-height: 1.5;">
                            AgriDron Solutions &middot; Lima, Perú. Si no solicitaste esta cuenta, puedes ignorar este mensaje.
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

    private async Task<string?> TryLoadLogoBase64Async(CancellationToken cancellationToken)
    {
        try
        {
            var candidatePaths = new[]
            {
                Path.Combine(hostEnvironment.ContentRootPath, "IAM", "Infrastructure", "Email", "Brevo", "Assets", LogoFileName),
                Path.Combine(AppContext.BaseDirectory, "IAM", "Infrastructure", "Email", "Brevo", "Assets", LogoFileName)
            };

            foreach (var path in candidatePaths)
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
                    if (bytes.Length > 0)
                    {
                        return Convert.ToBase64String(bytes);
                    }
                }
            }

            logger.LogWarning("Brand logo asset '{LogoFileName}' was not found. Sending email without inline logo.", LogoFileName);
            return null;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to read brand logo asset '{LogoFileName}'. Sending email without inline logo.", LogoFileName);
            return null;
        }
    }

    private static string BuildRoleIntroText(string encodedName, string role) =>
        role.Trim().ToUpperInvariant() switch
        {
            "FARMER" =>
                $"Hola {encodedName}, tu cuenta de agricultor en AgriDron Solutions ya está lista. Solo falta confirmar tu dirección de correo para empezar a gestionar tus fundos y parcelas.",
            "OPERATOR" =>
                $"Hola {encodedName}, tu cuenta de operador de drones en AgriDron Solutions ya está lista. Solo falta confirmar tu dirección de correo para acceder a tus misiones asignadas.",
            "ADMIN" =>
                $"Hola {encodedName}, tu cuenta de administrador en AgriDron Solutions ya está lista. Solo falta confirmar tu dirección de correo para acceder al panel de administración.",
            _ =>
                $"Hola {encodedName}, tu cuenta en AgriDron Solutions ya está lista. Solo falta confirmar tu dirección de correo para comenzar a usar la plataforma."
        };

    private sealed record BrevoEmailPayload(
        BrevoEmailParty Sender,
        BrevoEmailParty[] To,
        string Subject,
        string HtmlContent,
        BrevoAttachment[]? Attachment);

    private sealed record BrevoEmailParty(string Name, string Email);

    private sealed record BrevoAttachment(string Name, string Content);
}
