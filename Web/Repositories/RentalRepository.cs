using Dapper;
using Microsoft.Data.Sqlite;
using Shared.Models;


namespace Web.Repositories
{
    public class RentalRepository
    {
        private readonly string _connectionString;

        public RentalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection() =>
            new SqliteConnection(_connectionString);
        public async Task<IEnumerable<Rental>> GetByUserAsync(int userId, string sortBy = "StartedAt", string sortDir = "DESC")
        {
            var allowed = new[] { "StartedAt", "EndedAt", "Price", "DurationMinutes", "Status" };
            if (!allowed.Contains(sortBy)) sortBy = "StartedAt";
            if (sortDir != "ASC") sortDir = "DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Rental>(
                $"SELECT * FROM Rentals WHERE UserId = @UserId ORDER BY {sortBy} {sortDir}",
                new { UserId = userId });
        }
        public async Task<IEnumerable<Rental>> GetAllAsync(string sortBy = "Id", string sortDir = "ASC")
        {
            var allowed = new[] { "Id", "UserId", "BikeId", "StartStationId", "EndStationId", "StartedAt", "EndedAt", "DurationMinutes", "Price", "Status" };

            if (!allowed.Contains(sortBy)) sortBy = "Id";
            if (sortDir != "ASC") sortDir = "DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<Rental>($"SELECT * FROM Rentals ORDER BY {sortBy} {sortDir}");
        }

        public async Task<Rental?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Rental>(
                "SELECT * FROM Rentals WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Rental>> GetByUserAsync(int userId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Rental>(@"
            SELECT * FROM Rentals 
            WHERE UserId = @UserId 
            ORDER BY StartedAt DESC",
                new { UserId = userId });
        }

        public async Task<Rental?> GetActiveByUserAsync(int userId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Rental>(@"
            SELECT * FROM Rentals 
            WHERE UserId = @UserId AND Status = 'active'",
                new { UserId = userId });
        }

        public async Task<Rental?> GetActiveByBikeAsync(int bikeId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Rental>(@"
            SELECT * FROM Rentals
            WHERE BikeId = @BikeId AND Status = 'active'",
                new { BikeId = bikeId });
        }

        public async Task<int> CreateAsync(Rental rental)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO Rentals (UserId, BikeId, StartStationId, Status, StartedAt)
            VALUES (@UserId, @BikeId, @StartStationId, 'active', datetime('now'));
            SELECT last_insert_rowid();", rental);
        }

        public async Task ReturnAsync(int rentalId, int endStationId, decimal durationMinutes, decimal price)
        {
            DateTime endedAt = DateTime.UtcNow;
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Rentals
            SET EndStationId = @EndStationId,
                EndedAt = @EndedAt,
                DurationMinutes = @DurationMinutes,
                Price = @Price,
                Status = 'completed'
            WHERE Id = @RentalId AND Status = 'active'",
                new
                {
                    RentalId = rentalId,
                    EndStationId = endStationId,
                    EndedAt = endedAt,
                    DurationMinutes = durationMinutes,
                    Price = price
                });
        }
    }
}
