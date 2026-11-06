using Agridron.Backend.API.FieldManagement.Application.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Application.QueryServices;
using Agridron.Backend.API.Shared.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Mediator.Cortex.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using Agridron.Backend.API.Shared.Interfaces.Rest.ProblemDetails;
using Agridron.Backend.API.Shared.Resources;
using Agridron.Backend.API.Shared.Resources.Errors;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

// Add ProblemDetails services
builder.Services.AddProblemDetails();

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add Database Connection
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
    {
        throw new InvalidOperationException("Database connection string is not set in the configuration.");
    }

    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not set in the configuration.");
    }

    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors()
        .AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<DomainEventInterceptor>());

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Localization Configuration
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();

// Register the custom ProblemDetailsFactory
builder.Services.AddSingleton<ProblemDetailsFactory>();

// Register EF Core interceptors (scoped so they share the mediator per request)
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddScoped<DomainEventInterceptor>();

// Swagger / OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Agridron Platform API",
            Version = "v1",
            Description = "Agridron Platform Backend API"
        });
    options.EnableAnnotations();
});

// Dependency Injection - Shared Bounded Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Dependency Injection - Field Management Bounded Context
builder.Services.AddScoped<Agridron.Backend.API.FieldManagement.Domain.Repositories.IFarmRepository,
    Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories.FarmRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FieldManagement.Domain.Repositories.ICropRepository,
    Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories.CropRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FieldManagement.Domain.Repositories.IParcelRepository,
    Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories.ParcelRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FieldManagement.Domain.Repositories.IFumigationAreaRepository,
    Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories.FumigationAreaRepository>();

// Dependency Injection - Field Management application services
builder.Services.AddScoped<IFarmCommandService, FarmCommandService>();
builder.Services.AddScoped<IFarmQueryService, FarmQueryService>();
builder.Services.AddScoped<ICropCommandService, CropCommandService>();
builder.Services.AddScoped<ICropQueryService, CropQueryService>();
builder.Services.AddScoped<IParcelCommandService, ParcelCommandService>();
builder.Services.AddScoped<IParcelQueryService, ParcelQueryService>();
builder.Services.AddScoped<IFumigationAreaCommandService, FumigationAreaCommandService>();
builder.Services.AddScoped<IFumigationAreaQueryService, FumigationAreaQueryService>();
builder.Services.AddScoped<Agridron.Backend.API.FieldManagement.Interfaces.ACL.IFieldManagementContextFacade,
    Agridron.Backend.API.FieldManagement.Application.ACL.FieldManagementContextFacade>();

// Dependency Injection - Flight Operations Bounded Context repositories
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Domain.Repositories.IDroneRepository,
    Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories.DroneRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Domain.Repositories.IMissionRepository,
    Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories.MissionRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Domain.Repositories.IIncidentRepository,
    Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories.IncidentRepository>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Domain.Repositories.IOperationStatusRepository,
    Agridron.Backend.API.FlightOperations.Infrastructure.Persistence.EntityFrameworkCore.Repositories.OperationStatusRepository>();

// Dependency Injection - Flight Operations ACL & application services
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.Internal.OutboundServices.ACL.IExternalFieldService,
    Agridron.Backend.API.FlightOperations.Infrastructure.ACL.ExternalFieldService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.CommandServices.IDroneCommandService,
    Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices.DroneCommandService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.QueryServices.IDroneQueryService,
    Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices.DroneQueryService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.CommandServices.IMissionCommandService,
    Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices.MissionCommandService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.QueryServices.IMissionQueryService,
    Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices.MissionQueryService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.CommandServices.IIncidentCommandService,
    Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices.IncidentCommandService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.QueryServices.IIncidentQueryService,
    Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices.IncidentQueryService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.CommandServices.IOperationStatusCommandService,
    Agridron.Backend.API.FlightOperations.Application.Internal.CommandServices.OperationStatusCommandService>();
builder.Services.AddScoped<Agridron.Backend.API.FlightOperations.Application.QueryServices.IOperationStatusQueryService,
    Agridron.Backend.API.FlightOperations.Application.Internal.QueryServices.OperationStatusQueryService>();

// Mediator Configuration
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddCortexMediator([typeof(Program)]);

var app = builder.Build();

// Apply pending migrations on startup (safe to call even when schema is up to date)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "An error occurred while migrating the database. Ensure MySQL server is running.");
    }
}

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
