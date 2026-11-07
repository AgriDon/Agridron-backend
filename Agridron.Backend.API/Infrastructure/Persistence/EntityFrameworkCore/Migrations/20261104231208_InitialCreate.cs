using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Agridron.Backend.API.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "crops",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    variety = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_crops", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "farms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    owner_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_farms", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fumigation_areas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    parcel_id = table.Column<long>(type: "bigint", nullable: false),
                    geometry_json = table.Column<string>(type: "LONGTEXT", nullable: false),
                    area_hectares = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false, computedColumnSql: "ST_Area(ST_GeomFromGeoJSON(`geometry_json`)) / 10000", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_fumigation_areas", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "parcels",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    farm_id = table.Column<long>(type: "bigint", nullable: false),
                    crop_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    geometry_json = table.Column<string>(type: "LONGTEXT", nullable: false),
                    area_hectares = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false, computedColumnSql: "ST_Area(ST_GeomFromGeoJSON(`geometry_json`)) / 10000", stored: true),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_parcels", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ux_crops_name_variety",
                table: "crops",
                columns: new[] { "name", "variety" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_farms_name",
                table: "farms",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_farms_owner_id",
                table: "farms",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_fumigation_areas_parcel_id",
                table: "fumigation_areas",
                column: "parcel_id");

            migrationBuilder.CreateIndex(
                name: "ix_parcels_crop_id",
                table: "parcels",
                column: "crop_id");

            migrationBuilder.CreateIndex(
                name: "ix_parcels_farm_id",
                table: "parcels",
                column: "farm_id");

            // Native spatial columns generated from the GeoJSON strings by MySQL.
            migrationBuilder.Sql(
                "ALTER TABLE `parcels` ADD COLUMN `geometry_native` GEOMETRY GENERATED ALWAYS AS (ST_GeomFromGeoJSON(`geometry_json`)) STORED NOT NULL");

            migrationBuilder.Sql(
                "CREATE SPATIAL INDEX `sx_parcels_geometry` ON `parcels` (`geometry_native`)");

            migrationBuilder.Sql(
                "ALTER TABLE `fumigation_areas` ADD COLUMN `geometry_native` GEOMETRY GENERATED ALWAYS AS (ST_GeomFromGeoJSON(`geometry_json`)) STORED NOT NULL");

            migrationBuilder.Sql(
                "CREATE SPATIAL INDEX `sx_fumigation_areas_geometry` ON `fumigation_areas` (`geometry_native`)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "sx_parcels_geometry",
                table: "parcels");

            migrationBuilder.DropIndex(
                name: "sx_fumigation_areas_geometry",
                table: "fumigation_areas");

            migrationBuilder.Sql(
                "ALTER TABLE `parcels` DROP COLUMN `geometry_native`");

            migrationBuilder.Sql(
                "ALTER TABLE `fumigation_areas` DROP COLUMN `geometry_native`");

            migrationBuilder.DropTable(
                name: "crops");

            migrationBuilder.DropTable(
                name: "farms");

            migrationBuilder.DropTable(
                name: "fumigation_areas");

            migrationBuilder.DropTable(
                name: "parcels");
        }
    }
}
