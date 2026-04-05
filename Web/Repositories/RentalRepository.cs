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

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<Rental>("SELECT * FROM Rentals ORDER BY StartedAt DESC");
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
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Rentals
            SET EndStationId = @EndStationId,
                EndedAt = datetime('now'),
                DurationMinutes = @DurationMinutes,
                Price = @Price,
                Status = 'completed'
            WHERE Id = @RentalId AND Status = 'active'",
                new
                {
                    RentalId = rentalId,
                    EndStationId = endStationId,
                    DurationMinutes = durationMinutes,
                    Price = price
                });
        }
    }
}
