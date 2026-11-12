using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Agridron.Backend.API.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightOperationsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    serial_number = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    capacity_liters = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    battery_level = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    flight_hours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    nozzle_id = table.Column<long>(type: "bigint", nullable: true),
                    last_maintenance_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    next_maintenance_date = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drones", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    mission_id = table.Column<long>(type: "bigint", nullable: true),
                    drone_id = table.Column<long>(type: "bigint", nullable: true),
                    severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_incidents", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "missions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    farm_id = table.Column<long>(type: "bigint", nullable: true),
                    parcel_id = table.Column<long>(type: "bigint", nullable: true),
                    drone_id = table.Column<long>(type: "bigint", nullable: true),
                    chemical_id = table.Column<long>(type: "bigint", nullable: true),
                    operator_id = table.Column<long>(type: "bigint", nullable: true),
                    farm_area = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    crop_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    operator_name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    treatment_type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    product_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    product_dose = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    planned_area_hectares = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    treated_area_hectares = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    progress = table.Column<int>(type: "int", nullable: false),
                    flight_altitude_meters = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    flight_speed_mps = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    flight_spray_rate_lph = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    flight_overlap_percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    last_pause_latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    last_pause_longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_missions", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "operation_statuses",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    mission_id = table.Column<long>(type: "bigint", nullable: true),
                    drone_id = table.Column<long>(type: "bigint", nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    altitude_meters = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    speed_mps = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    battery_level = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    tank_volume_liters = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    status = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_operation_statuses", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_drones_status",
                table: "drones",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_drones_serial_number",
                table: "drones",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_incidents_drone_id",
                table: "incidents",
                column: "drone_id");

            migrationBuilder.CreateIndex(
                name: "ix_incidents_mission_id",
                table: "incidents",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_incidents_severity",
                table: "incidents",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_missions_drone_id",
                table: "missions",
                column: "drone_id");

            migrationBuilder.CreateIndex(
                name: "ix_missions_farm_id",
                table: "missions",
                column: "farm_id");

            migrationBuilder.CreateIndex(
                name: "ix_missions_parcel_id",
                table: "missions",
                column: "parcel_id");

            migrationBuilder.CreateIndex(
                name: "ix_missions_scheduled_at",
                table: "missions",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "ix_missions_status",
                table: "missions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_missions_code",
                table: "missions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operation_statuses_drone_id",
                table: "operation_statuses",
                column: "drone_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_statuses_mission_id",
                table: "operation_statuses",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_operation_statuses_timestamp",
                table: "operation_statuses",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "drones");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "missions");

            migrationBuilder.DropTable(
                name: "operation_statuses");
        }
    }
}
