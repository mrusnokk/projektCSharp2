using Dapper;
using Microsoft.Data.Sqlite;
using Shared.Models;
namespace Web.Repositories
{
    public class BikeStatusHistoryRepository
    {
        private readonly string _connectionString;

        public BikeStatusHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection() =>
            new SqliteConnection(_connectionString);

        public async Task<IEnumerable<BikeStatusHistory>> GetByBikeAsync(int bikeId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<BikeStatusHistory>(@"
            SELECT * FROM BikeStatusHistory 
            WHERE BikeId = @BikeId 
            ORDER BY ChangedAt DESC",
                new { BikeId = bikeId });
        }

        public async Task AddAsync(BikeStatusHistory history)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            INSERT INTO BikeStatusHistory 
                (BikeId, OldStatus, NewStatus, StationId, RentalId, ChangedAt, Note)
            VALUES 
                (@BikeId, @OldStatus, @NewStatus, @StationId, @RentalId, datetime('now'), @Note)",
                history);
        }
    }
}
