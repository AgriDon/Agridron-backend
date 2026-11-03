using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Validation;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

/// <summary>
///     An agricultural crop that can be assigned to a parcel.
/// </summary>
public sealed class Crop : AggregateRoot<CropId>
{
    /// <summary>Maximum length of the crop name.</summary>
    public const int MaxNameLength = 80;

    /// <summary>Maximum length of the crop variety.</summary>
    public const int MaxVarietyLength = 80;

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    private Crop()
    {
    }

    /// <summary>
    ///     Creates a crop.
    /// </summary>
    /// <param name="name">The crop name.</param>
    /// <param name="variety">The crop variety.</param>
    /// <exception cref="Shared.Domain.Model.Exceptions.DomainException">
    ///     When the name or the variety are missing or too long.
    /// </exception>
    public Crop(string name, string variety) : base(CropId.Empty)
    {
        UpdateDetails(name, variety);
    }

    /// <summary>The crop name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The crop variety.</summary>
    public string Variety { get; private set; } = string.Empty;

    /// <summary>
    ///     Updates the crop name and variety.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="variety">The new variety.</param>
    public void UpdateDetails(string name, string variety)
    {
        Name = FieldManagementGuard.RequiredText(
            name,
            FieldManagementErrors.CropNameRequired,
            MaxNameLength,
            FieldManagementErrors.CropNameTooLong);

        Variety = FieldManagementGuard.RequiredText(
            variety,
            FieldManagementErrors.CropVarietyRequired,
            MaxVarietyLength,
            FieldManagementErrors.CropVarietyTooLong);
    }
}
