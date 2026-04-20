using Microsoft.Data.Sqlite;
using Shared.Models;
using Dapper;
namespace Web.Repositories
{
    public class BikeRepository
    {
        private readonly string _connectionString;

        public BikeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection() =>
            new SqliteConnection(_connectionString);
        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            using var connection = CreateConnection();
            if (excludeId.HasValue)
                return await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Bikes WHERE Code = @Code AND Id != @ExcludeId",
                    new { Code = code, ExcludeId = excludeId }) > 0;

            return await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Bikes WHERE Code = @Code",
                new { Code = code }) > 0;
        }
        public async Task<IEnumerable<Bike>> GetAllAsync(string sortBy = "Code", string sortDir = "ASC")
        {
            var allowed = new[] { "Code", "Model", "Status" };
            if (!allowed.Contains(sortBy)) sortBy = "Code";
            if (sortDir != "ASC") sortDir = "DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Bike>(
                $"SELECT * FROM Bikes ORDER BY {sortBy} {sortDir}");
        }

        public async Task<Bike?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Bike>(
                "SELECT * FROM Bikes WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Bike>> GetByStationAsync(int stationId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Bike>(
                "SELECT * FROM Bikes WHERE CurrentStationId = @StationId",
                new { StationId = stationId });
        }

        public async Task<IEnumerable<Bike>> GetAvailableByStationAsync(int stationId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Bike>(
                "SELECT * FROM Bikes WHERE CurrentStationId = @StationId AND Status = 'available'",
                new { StationId = stationId });
        }

        public async Task<int> CreateAsync(Bike bike)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO Bikes (Code, Model, CurrentStationId, Status)
            VALUES (@Code, @Model, @CurrentStationId, @Status);
            SELECT last_insert_rowid();", bike);
        }

        public async Task UpdateAsync(Bike bike)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Bikes
            SET Code = @Code, Model = @Model,
                CurrentStationId = @CurrentStationId, Status = @Status
            WHERE Id = @Id", bike);
        }

        public async Task UpdateStatusAsync(int bikeId, string newStatus, int? stationId = null)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Bikes
            SET Status = @NewStatus, CurrentStationId = @StationId
            WHERE Id = @BikeId",
                new { BikeId = bikeId, NewStatus = newStatus, StationId = stationId });
        }
        public async Task DeleteAsync(int bikeId)
        {
            using var connection = CreateConnection();

            // Zkontroluj jestli kolo má nějaká půjčení
            var rentalCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Rentals WHERE BikeId = @Id",
                new { Id = bikeId });

            if (rentalCount > 0)
                throw new Exception("Nelze smazat kolo které má historická půjčení.");

            await connection.ExecuteAsync(
                "DELETE FROM Bikes WHERE Id = @Id",
                new { Id = bikeId });
        }
    }
}
