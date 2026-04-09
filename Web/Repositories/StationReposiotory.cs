using Dapper;
using Microsoft.Data.Sqlite;
using Shared.Models;
namespace Web.Repositories
{
    public class StationRepository
    {
        private readonly string _connectionString;

        public StationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection() =>
            new SqliteConnection(_connectionString);

        public async Task<IEnumerable<Station>> GetAllAsync(string sortBy = "Name", string sortDir = "ASC")
        {
            var allowed = new[] { "Name", "Address", "Capacity" };
            if (!allowed.Contains(sortBy)) sortBy = "Name";
            if (sortDir != "ASC") sortDir = "DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Station>(
                $"SELECT * FROM Stations WHERE IsActive = 1 ORDER BY {sortBy} {sortDir}");
        }

        public async Task<Station?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Station>(
                "SELECT * FROM Stations WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Station station)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO Stations (Name, Lat, Lng, Address, Capacity, IsActive)
            VALUES (@Name, @Lat, @Lng, @Address, @Capacity, @IsActive);
            SELECT last_insert_rowid();", station);
        }

        public async Task UpdateAsync(Station station)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Stations 
            SET Name = @Name, Lat = @Lat, Lng = @Lng, 
                Address = @Address, Capacity = @Capacity, IsActive = @IsActive
            WHERE Id = @Id", station);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Stations SET IsActive = 0 WHERE Id = @Id", new { Id = id });
        }
    }
}
