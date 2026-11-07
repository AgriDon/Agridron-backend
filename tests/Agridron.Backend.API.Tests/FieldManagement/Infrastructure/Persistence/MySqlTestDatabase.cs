using MySql.Data.MySqlClient;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

/// <summary>
///     Locates and prepares the MySQL database used by the persistence tests.
/// </summary>
/// <remarks>
///     The connection string can be overridden with the <c>AGRIDRON_TEST_CONNECTION</c>
///     environment variable, which is handy on CI where a service container is used.
/// </remarks>
internal static class MySqlTestDatabase
{
    private const string DefaultConnectionString =
        "server=localhost;port=3306;user=root;password=root;database=agridron_test_db";

    private static readonly Lazy<bool> Availability = new(CheckAvailability);

    /// <summary>The connection string used by the persistence tests.</summary>
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("AGRIDRON_TEST_CONNECTION") ?? DefaultConnectionString;

    /// <summary>Whether a MySQL server is reachable with the test connection string.</summary>
    public static bool IsAvailable => Availability.Value;

    /// <summary>Creates the test database when it does not exist yet.</summary>
    public static void EnsureDatabase()
    {
        var builder = new MySqlConnectionStringBuilder(ConnectionString);
        var database = builder.Database;

        if (string.IsNullOrWhiteSpace(database))
        {
            throw new InvalidOperationException("The test connection string must include a database name.");
        }

        builder.Database = string.Empty;

        using var connection = new MySqlConnection(builder.ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"CREATE DATABASE IF NOT EXISTS `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
        command.ExecuteNonQuery();
    }

    private static bool CheckAvailability()
    {
        try
        {
            EnsureDatabase();
            return true;
        }
        catch (MySqlException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
