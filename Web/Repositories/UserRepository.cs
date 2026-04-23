using Dapper;
using Microsoft.Data.Sqlite;
using Shared.Models;
namespace Web.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqliteConnection CreateConnection() =>
            new SqliteConnection(_connectionString);

        public async Task<IEnumerable<User>> GetAllAsync(string sortBy = "Id", string sortDir = "ASC")
        {
            var allowed = new[] { "Id", "Email", "FirstName", "LastName", "Phone", "CreatedAt", "IsActive" };

            if (!allowed.Contains(sortBy)) sortBy = "Id";
            if (sortDir != "ASC") sortDir = "DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<User>($"SELECT * FROM Users ORDER BY {sortBy} {sortDir}");
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(@"
            INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, CreatedAt, IsActive)
            VALUES (@Email, @PasswordHash, @FirstName, @LastName, @Phone, datetime('now'), 1);
            SELECT last_insert_rowid();", user);
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(@"
            UPDATE Users
            SET Email = @Email,
                FirstName = @FirstName,
                LastName = @LastName,
                Phone = @Phone,
                IsActive = @IsActive
            WHERE Id = @Id", user);
        }

        public async Task SetActiveAsync(int userId, bool isActive)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Users SET IsActive = @IsActive WHERE Id = @Id",
                new { Id = userId, IsActive = isActive ? 1 : 0 });
        }
        public async Task DeleteAsync(int userId)
        {
            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Users SET IsActive = 0 WHERE Id = @Id",
                new { Id = userId });
        }
    }
}
