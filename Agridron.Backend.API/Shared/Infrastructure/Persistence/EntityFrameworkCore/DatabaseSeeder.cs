using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore;

/// <summary>
///     Seeds initial demonstration and testing data into the database if it is empty.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Only seed if no farms exist yet
            if (await context.Set<Farm>().AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding initial demo data for Agridron Platform...");

            // 1. Seed Crops
            var cropUva = new Crop("Uva", "Red Globe");
            var cropFresa = new Crop("Fresa", "Camarosa");
            var cropPalto = new Crop("Palto", "Hass");
            var cropArandano = new Crop("Arándano", "Biloxi");

            await context.Set<Crop>().AddRangeAsync(cropUva, cropFresa, cropPalto, cropArandano);

            // 2. Seed Farms
            var farmSantaMaria = new Farm(
                "Finca Santa María",
                "Valle de Ica, KM 284",
                1,
                "https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=600");

            var farmVinedos = new Farm(
                "Finca Los Viñedos",
                "Pisco, Sector Buena Vista",
                1,
                "https://images.unsplash.com/photo-1500937386664-56d1dfef3854?w=600");

            await context.Set<Farm>().AddRangeAsync(farmSantaMaria, farmVinedos);
            await context.SaveChangesAsync();

            // 3. Seed Parcels with valid GeoJSON Polygons
            var polygon1 = GeoJsonPolygon.FromGeoJson(
                "{\"type\":\"Polygon\",\"coordinates\":[[[-75.728,-14.068],[-75.728,-14.065],[-75.724,-14.065],[-75.724,-14.068],[-75.728,-14.068]]]}");
            var parcel1 = new Parcel(
                farmSantaMaria.Id,
                cropUva.Id,
                "Lote 1 - Uva de Mesa",
                polygon1,
                "Floración",
                "https://images.unsplash.com/photo-1500937386664-56d1dfef3854?w=600");

            var polygon2 = GeoJsonPolygon.FromGeoJson(
                "{\"type\":\"Polygon\",\"coordinates\":[[[-75.732,-14.072],[-75.732,-14.069],[-75.729,-14.069],[-75.729,-14.072],[-75.732,-14.072]]]}");
            var parcel2 = new Parcel(
                farmSantaMaria.Id,
                cropFresa.Id,
                "Lote 2 - Fresa Silvestre",
                polygon2,
                "Fructificación",
                "https://images.unsplash.com/photo-1465146344425-f00d5f5c8f07?w=600");

            var polygon3 = GeoJsonPolygon.FromGeoJson(
                "{\"type\":\"Polygon\",\"coordinates\":[[[-76.195,-13.715],[-76.195,-13.710],[-76.190,-13.710],[-76.190,-13.715],[-76.195,-13.715]]]}");
            var parcel3 = new Parcel(
                farmVinedos.Id,
                cropPalto.Id,
                "Parcela Central - Palto Hass",
                polygon3,
                "Crecimiento vegetativo",
                "https://images.unsplash.com/photo-1521405924368-64c5b84bec60?w=600");

            await context.Set<Parcel>().AddRangeAsync(parcel1, parcel2, parcel3);
            await context.SaveChangesAsync();

            // 4. Seed Fumigation Areas
            var fumigationArea1 = new FumigationArea(parcel1.Id, polygon1);
            await context.Set<FumigationArea>().AddAsync(fumigationArea1);

            // 5. Seed Drones
            var drone1 = new Drone(
                "AGRI-DR-001",
                "DJI Agras T40",
                40.0m,
                null,
                "https://images.unsplash.com/photo-1527977966376-1c8408f9f108?w=600");

            var drone2 = new Drone(
                "AGRI-DR-002",
                "DJI Agras T30",
                30.0m,
                null,
                "https://images.unsplash.com/photo-1508614589041-895b88991e3e?w=600");

            var drone3 = new Drone(
                "AGRI-DR-003",
                "XAG P100 Pro",
                50.0m,
                null,
                "https://images.unsplash.com/photo-1521405924368-64c5b84bec60?w=600");

            await context.Set<Drone>().AddRangeAsync(drone1, drone2, drone3);
            await context.SaveChangesAsync();

            // 6. Seed Missions
            var mission1 = new Mission(
                "MIS-2026-001",
                farmSantaMaria.Id.Value,
                parcel1.Id.Value,
                "Lote 1 - Uva de Mesa",
                "Uva",
                DateTimeOffset.UtcNow.AddHours(2),
                12.5m,
                "Carlos Mendoza",
                "Fungicida",
                "Sulfato de Cobre",
                2.5m,
                "Fumigación preventiva matutina contra mildiu.",
                drone1.Id,
                0);

            var mission2 = new Mission(
                "MIS-2026-002",
                farmSantaMaria.Id.Value,
                parcel2.Id.Value,
                "Lote 2 - Fresa Silvestre",
                "Fresa",
                DateTimeOffset.UtcNow.AddDays(-1),
                8.0m,
                "Carlos Mendoza",
                "Insecticida",
                "Abamectina 1.8%",
                1.2m,
                "Control de arañita roja.",
                drone2.Id,
                100);

            await context.Set<Mission>().AddRangeAsync(mission1, mission2);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
