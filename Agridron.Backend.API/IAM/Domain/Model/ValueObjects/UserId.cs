using System.Globalization;

namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>Strongly-typed identifier for a <c>User</c> aggregate.</summary>
public readonly record struct UserId(long Value)
{
    public static UserId Empty => new(0);

    /// <summary>Creates a validated <see cref="UserId" /> from a positive database identifier.</summary>
    public static UserId From(long value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "El identificador de usuario debe ser mayor que cero.");
        }

        return new UserId(value);
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}
