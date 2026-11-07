namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

/// <summary>
///     A <see cref="FactAttribute" /> that is skipped when no MySQL server is reachable.
/// </summary>
public sealed class MySqlFactAttribute : FactAttribute
{
    /// <summary>
    ///     Initializes the attribute, skipping the test when the database is not available.
    /// </summary>
    public MySqlFactAttribute()
    {
        if (!MySqlTestDatabase.IsAvailable)
        {
            Skip = "MySQL is not available. Set AGRIDRON_TEST_CONNECTION to run the persistence tests.";
        }
    }
}
