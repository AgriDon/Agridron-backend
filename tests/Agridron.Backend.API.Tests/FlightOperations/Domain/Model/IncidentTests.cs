using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.Errors;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;
using FluentAssertions;

namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

public sealed class IncidentTests
{
    [Fact]
    public void Constructor_WithValidFields_ShouldSucceed()
    {
        var now = DateTimeOffset.UtcNow;
        var incident = new Incident(
            type: "ALERTA_CLIMA",
            description: "Viento superior a 15 km/h",
            occurredAt: now,
            missionId: new MissionId(1),
            severity: IncidentSeverity.High);

        incident.Type.Should().Be("ALERTA_CLIMA");
        incident.Description.Should().Be("Viento superior a 15 km/h");
        incident.Severity.Should().Be(IncidentSeverity.High);
        incident.MissionId.Should().Be(new MissionId(1));
        incident.OccurredAt.Should().Be(now);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WithMissingType_ShouldThrow(string? type)
    {
        var act = () => new Incident(type!, "Desc", DateTimeOffset.UtcNow);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.IncidentTypeRequired.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WithMissingDescription_ShouldThrow(string? desc)
    {
        var act = () => new Incident("TYPE", desc!, DateTimeOffset.UtcNow);
        act.Should().Throw<DomainException>()
            .Which.Error.Code.Should().Be(FlightOperationsErrors.IncidentDescriptionRequired.Code);
    }
}
