namespace Agridron.Backend.API.Tests.FlightOperations.Domain.Model;

using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;


public class MaintenanceRecordTests {
        [Fact]
        public void Constructor_WithValidData_ShouldTrimTexts()
        {
            // Arrange
            var date = new DateTimeOffset(2026, 11, 15, 0, 0, 0, TimeSpan.Zero);

            // Act
            var record = new MaintenanceRecord(new DroneId(1), date, MaintenanceType.Corrective, "  Lucía Torres  ", "  Cambio de motor  ");

            // Assert
            record.DroneId.Value.Should().Be(1);
            record.PerformedAt.Should().Be(date);
            record.Type.Should().Be(MaintenanceType.Corrective);
            record.Technician.Should().Be("Lucía Torres");
            record.Description.Should().Be("Cambio de motor");
        }

        [Fact]
        public void Constructor_WithoutDescription_ShouldBeNull()
        {
            // Act
            var record = new MaintenanceRecord(new DroneId(1), DateTimeOffset.UtcNow, MaintenanceType.Preventive, "Carlos Ramírez", "   ");

            // Assert
            record.Description.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithoutTechnician_ShouldThrowValidation(string? technician)
        {
            // Act
            var act = () => new MaintenanceRecord(new DroneId(1), DateTimeOffset.UtcNow, MaintenanceType.Preventive, technician!);

            // Assert
            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("MaintenanceTechnicianRequired");
        }

        [Fact]
        public void Constructor_WithTooLongTechnician_ShouldThrowValidation()
        {
            // Act
            var act = () => new MaintenanceRecord(new DroneId(1), DateTimeOffset.UtcNow, MaintenanceType.Preventive, new string('x', 121));

            // Assert
            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("MaintenanceTechnicianTooLong");
        }

        [Fact]
        public void Constructor_WithInvalidType_ShouldThrowValidation()
        {
            // Act
            var act = () => new MaintenanceRecord(new DroneId(1), DateTimeOffset.UtcNow, (MaintenanceType)99, "Carlos Ramírez");

            // Assert
            act.Should().Throw<DomainException>()
                .Which.Error.Code.Should().Be("MaintenanceTypeInvalid");
        }
}
