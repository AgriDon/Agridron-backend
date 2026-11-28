using System.Net;
using System.Text;
using System.Text.Json;
using Agridron.Backend.API.IAM.Infrastructure.Email.Brevo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agridron.Backend.API.Tests.IAM.Infrastructure;

public class BrevoEmailServiceTests
{
    [Fact]
    public async Task SendWelcomeEmailAsync_WhenLogoExists_ShouldSendInlineCidAttachmentAndFarmerCopyAsync()
    {
        // Arrange
        var tempRoot = CreateTempDirectoryWithLogo(out var logoBytes);
        try
        {
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.Created);
            var service = CreateService(handler, tempRoot, enabled: true);

            // Act
            await service.SendWelcomeEmailAsync("farmer@agridron.pe", "Carlos <Perez>", "Farmer", "tok-123");

            // Assert
            handler.CapturedRequests.Should().ContainSingle();
            var captured = handler.CapturedRequests[0];
            captured.ApiKeyHeader.Should().Be("test-brevo-api-key");

            using var doc = JsonDocument.Parse(captured.Body);
            var root = doc.RootElement;
            root.GetProperty("subject").GetString().Should().Be("Confirma tu correo electrónico — AgriDron Solutions");

            var attachments = root.GetProperty("attachment");
            attachments.GetArrayLength().Should().Be(1);
            attachments[0].GetProperty("name").GetString().Should().Be(BrevoEmailService.LogoFileName);
            attachments[0].GetProperty("content").GetString().Should().Be(Convert.ToBase64String(logoBytes));

            var html = root.GetProperty("htmlContent").GetString()!;
            html.Should().Contain($"src=\"cid:{BrevoEmailService.LogoFileName}\"");
            html.Should().Contain("Confirma tu correo electrónico");
            html.Should().Contain("Hola Carlos &lt;Perez&gt;, tu cuenta de agricultor en AgriDron Solutions ya está lista.");
            html.Should().Contain("Confirmar correo electrónico");
            html.Should().Contain("http://localhost:5173/verify-email?token=tok-123");
            html.Should().Contain("#15803d");
            html.Should().Contain("AgriDron Solutions &middot; Lima, Perú.");
            html.Should().NotContain("Rol asignado");
            html.Should().NotContain("Precisión Agrícola");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WhenLogoIsMissing_ShouldSendWithoutAttachmentAndUseOperatorCopyAsync()
    {
        // Arrange
        var emptyRoot = Path.Combine(Path.GetTempPath(), $"agridron-no-logo-{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptyRoot);
        try
        {
            var handler = new RecordingHttpMessageHandler(HttpStatusCode.Created);
            var service = CreateService(handler, emptyRoot, enabled: true);

            // Act
            await service.SendWelcomeEmailAsync("op@agridron.pe", "Lucia Torres", "Operator", "tok-456");

            // Assert
            handler.CapturedRequests.Should().ContainSingle();
            using var doc = JsonDocument.Parse(handler.CapturedRequests[0].Body);
            var root = doc.RootElement;
            root.TryGetProperty("attachment", out _).Should().BeFalse();

            var html = root.GetProperty("htmlContent").GetString()!;
            html.Should().NotContain("cid:");
            html.Should().Contain("Hola Lucia Torres, tu cuenta de operador de drones en AgriDron Solutions ya está lista.");
            html.Should().Contain("http://localhost:5173/verify-email?token=tok-456");
        }
        finally
        {
            Directory.Delete(emptyRoot, recursive: true);
        }
    }

    [Fact]
    public async Task SendWelcomeEmailAsync_WhenDisabled_ShouldNotSendHttpRequestAsync()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, Path.GetTempPath(), enabled: false);

        // Act
        await service.SendWelcomeEmailAsync("farmer@agridron.pe", "Carlos", "Farmer", "tok-789");

        // Assert
        handler.CapturedRequests.Should().BeEmpty();
    }

    private static BrevoEmailService CreateService(
        RecordingHttpMessageHandler handler,
        string contentRootPath,
        bool enabled)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Brevo:Enabled"] = enabled.ToString(),
                ["Brevo:ApiKey"] = "test-brevo-api-key",
                ["Brevo:SenderEmail"] = "noreply@agridron.pe",
                ["Brevo:SenderName"] = "AgriDron Solutions",
                ["Brevo:FrontendBaseUrl"] = "http://localhost:5173"
            })
            .Build();

        var httpClientFactory = new StubHttpClientFactory(handler);
        var hostEnvironment = new StubHostEnvironment(contentRootPath);

        return new BrevoEmailService(
            httpClientFactory,
            configuration,
            hostEnvironment,
            NullLogger<BrevoEmailService>.Instance);
    }

    private static string CreateTempDirectoryWithLogo(out byte[] logoBytes)
    {
        var root = Path.Combine(Path.GetTempPath(), $"agridron-logo-{Guid.NewGuid():N}");
        var assetDir = Path.Combine(root, "IAM", "Infrastructure", "Email", "Brevo", "Assets");
        Directory.CreateDirectory(assetDir);

        logoBytes = Encoding.UTF8.GetBytes("fake-png-content");
        File.WriteAllBytes(Path.Combine(assetDir, BrevoEmailService.LogoFileName), logoBytes);
        return root;
    }

    private sealed record CapturedRequest(string? ApiKeyHeader, string Body);

    private sealed class RecordingHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public List<CapturedRequest> CapturedRequests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var apiKey = request.Headers.TryGetValues("api-key", out var values)
                ? values.FirstOrDefault()
                : null;

            CapturedRequests.Add(new CapturedRequest(apiKey, body));
            return new HttpResponseMessage(statusCode);
        }
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://api.brevo.com/v3/smtp/email")
            };
    }

    private sealed class StubHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Agridron.Backend.API.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
