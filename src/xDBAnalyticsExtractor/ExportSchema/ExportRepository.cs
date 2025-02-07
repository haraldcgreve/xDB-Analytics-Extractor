using Dapper;
using Microsoft.Data.Sqlite;
using xDBAnalyticsExtractor.Utils;

namespace xDBAnalyticsExtractor.ExportSchema;

public class ExportRepository
{
    private static string _sqliteDbPath = string.Empty;
    private static string _connectionString = string.Empty;

    public ExportRepository(IConfiguration configuration)
    {
        _sqliteDbPath = configuration.GetValue<string>("InternalDBPath") ?? string.Empty;
        _connectionString = $"Data Source={_sqliteDbPath};";

        IOUtils.EnsureParentDirectoriesExist(_sqliteDbPath);
        InitializeDatabase();
    }

    public ExportRepository()
    {
    }

    private static void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Exports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LastExported DATETIME
                )");
        }
    }
    public void InsertExport(Export export)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string query = "INSERT INTO Exports (LastExported) VALUES (@LastExported);";
        connection.Execute(query, export);
        connection.Close();
    }

    public Export? GetLatestExport()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string query = "SELECT ID, LastExported FROM Exports ORDER BY LastExported DESC LIMIT 1;";
        var export = connection.QueryFirstOrDefault<Export>(query);
        connection.Close();
        return export;
    }

    public Export? GetEarliestExport()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        string query = "SELECT ID, LastExported FROM Exports ORDER BY LastExported ASC LIMIT 1;";
        var export = connection.QueryFirstOrDefault<Export>(query);
        connection.Close();
        return export;
    }
}
