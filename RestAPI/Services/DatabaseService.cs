using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestAPI.Interfaces;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _logger = logger;
    }

    public async Task<IDbConnection> GetConnection()
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Database connection opened successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open database connection");
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, object parameters = null) where T : new()
    {
        using var connection = await GetConnection();
        using var command = new SqlCommand(storedProcedure, (SqlConnection)connection);
        command.CommandType = CommandType.StoredProcedure;

        // Add parameters if provided
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<T>();

        while (await reader.ReadAsync())
        {
            var item = new T();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                try
                {
                    var value = reader[property.Name];
                    if (value != DBNull.Value)
                    {
                        property.SetValue(item, value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to map property {property.Name}");
                }
            }

            results.Add(item);
        }

        _logger.LogInformation($"Stored procedure {storedProcedure} returned {results.Count} records");
        return results;
    }

    public async Task<T> QuerySingleAsync<T>(string storedProcedure, object parameters = null) where T : new()
    {
        var results = await QueryAsync<T>(storedProcedure, parameters);
        return results.FirstOrDefault();
    }

    public async Task<int> ExecuteAsync(string storedProcedure, object parameters = null)
    {
        using var connection = await GetConnection();
        using var command = new SqlCommand(storedProcedure, (SqlConnection)connection);
        command.CommandType = CommandType.StoredProcedure;

        // Add parameters if provided
        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        var result = await command.ExecuteNonQueryAsync();
        _logger.LogInformation($"Stored procedure {storedProcedure} affected {result} rows");
        return result;
    }

    public async Task<object> ExecuteScalarAsync(string storedProcedure, object parameters = null)
    {
        using var connection = await GetConnection();
        using var command = new SqlCommand(storedProcedure, (SqlConnection)connection);
        command.CommandType = CommandType.StoredProcedure;

        if (parameters != null)
        {
            AddParameters(command, parameters);
        }

        var result = await command.ExecuteScalarAsync();
        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = await GetConnection();
            using var command = new SqlCommand("sp_TestConnection", (SqlConnection)connection);
            command.CommandType = CommandType.StoredProcedure;
            var result = await command.ExecuteScalarAsync();
            _logger.LogInformation("Database connection test successful");
            return result != null && (int)result == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return false;
        }
    }

    public async Task<int> GetUserCountAsync()
    {
        using var connection = await GetConnection();
        using var command = new SqlCommand("sp_GetUserCount", (SqlConnection)connection);
        command.CommandType = CommandType.StoredProcedure;
        var result = await command.ExecuteScalarAsync();
        var count = result != null ? Convert.ToInt32(result) : 0;
        _logger.LogInformation($"Total users in database: {count}");
        return count;
    }

    // Helper method to add parameters to command
    private void AddParameters(SqlCommand command, object parameters)
    {
        foreach (var prop in parameters.GetType().GetProperties())
        {
            var param = new SqlParameter($"@{prop.Name}", prop.GetValue(parameters) ?? DBNull.Value);
            command.Parameters.Add(param);
        }
    }
}