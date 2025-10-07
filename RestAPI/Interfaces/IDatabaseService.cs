using System.Data;

namespace RestAPI.Interfaces
{
    public interface IDatabaseService
    {
        Task<IDbConnection> GetConnection();
        Task<IEnumerable<T>> QueryAsync<T>(string storedProcedure, object parameters = null) where T : new();
        Task<T> QuerySingleAsync<T>(string storedProcedure, object parameters = null) where T : new();
        Task<int> ExecuteAsync(string storedProcedure, object parameters = null);
        Task<object> ExecuteScalarAsync(string storedProcedure, object parameters = null);
        Task<bool> TestConnectionAsync();
        Task<int> GetUserCountAsync();
    }
}
