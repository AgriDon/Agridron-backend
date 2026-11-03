using Agridron.Backend.API.FieldManagement.Domain.Model.Errors;
using Agridron.Backend.API.FieldManagement.Domain.Model.Validation;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.Shared.Domain.Model.Aggregates;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;

/// <summary>
///     An agricultural farm. It is the top level entity of the field management model and owns
///     the parcels where missions are executed.
/// </summary>
public sealed class Farm : AggregateRoot<FarmId>
{
    /// <summary>Maximum length of the farm name.</summary>
    public const int MaxNameLength = 120;

    /// <summary>Maximum length of the farm location.</summary>
    public const int MaxLocationLength = 200;

    /// <summary>Maximum length of the farm image URL.</summary>
    public const int MaxImageUrlLength = 500;

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    private Farm()
    {
    }

    /// <summary>
    ///     Creates a farm.
    /// </summary>
    /// <param name="name">The farm name.</param>
    /// <param name="location">The farm location.</param>
    /// <param name="ownerId">The identifier of the user that owns the farm (IAM bounded context).</param>
    /// <param name="imageUrl">An optional image URL.</param>
    public Farm(string name, string location, long ownerId, string? imageUrl = null) : base(FarmId.Empty)
    {
        Rename(name);
        Relocate(location);
        ChangeOwner(ownerId);
        ChangeImage(imageUrl);
    }

    /// <summary>The farm name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The farm location.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>
    ///     The identifier of the user that owns the farm. It belongs to the IAM bounded context,
    ///     so it is stored as a plain identifier without a foreign key.
    /// </summary>
    public long OwnerId { get; private set; }

    /// <summary>An optional image URL.</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>Renames the farm.</summary>
    /// <param name="name">The new name.</param>
    public void Rename(string name)
    {
        Name = FieldManagementGuard.RequiredText(
            name,
            FieldManagementErrors.FarmNameRequired,
            MaxNameLength,
            FieldManagementErrors.FarmNameTooLong);
    }

    /// <summary>Changes the farm location.</summary>
    /// <param name="location">The new location.</param>
    public void Relocate(string location)
    {
        Location = FieldManagementGuard.RequiredText(
            location,
            FieldManagementErrors.FarmLocationRequired,
            MaxLocationLength,
            FieldManagementErrors.FarmLocationTooLong);
    }

    /// <summary>Changes the farm owner.</summary>
    /// <param name="ownerId">The new owner identifier.</param>
    public void ChangeOwner(long ownerId)
    {
        OwnerId = FieldManagementGuard.RequiredId(ownerId, FieldManagementErrors.FarmOwnerInvalid);
    }

    /// <summary>Changes or clears the farm image.</summary>
    /// <param name="imageUrl">The new image URL, or null to clear it.</param>
    public void ChangeImage(string? imageUrl)
    {
        ImageUrl = FieldManagementGuard.OptionalUrl(
            imageUrl,
            MaxImageUrlLength,
            FieldManagementErrors.FarmImageUrlInvalid);
    }
}
