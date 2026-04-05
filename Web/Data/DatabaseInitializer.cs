using Dapper;
using Microsoft.Data.Sqlite;

namespace Web.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Phone TEXT,
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Stations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Lat REAL NOT NULL,
                Lng REAL NOT NULL,
                Address TEXT,
                Capacity INTEGER NOT NULL DEFAULT 10,
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Bikes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Code TEXT NOT NULL UNIQUE,
                Model TEXT NOT NULL,
                CurrentStationId INTEGER REFERENCES Stations(Id),
                Status TEXT NOT NULL DEFAULT 'available',
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Rentals (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL REFERENCES Users(Id),
                BikeId INTEGER NOT NULL REFERENCES Bikes(Id),
                StartStationId INTEGER NOT NULL REFERENCES Stations(Id),
                EndStationId INTEGER REFERENCES Stations(Id),
                StartedAt TEXT NOT NULL DEFAULT (datetime('now')),
                EndedAt TEXT,
                DurationMinutes REAL,
                Price REAL,
                Status TEXT NOT NULL DEFAULT 'active'
            );

            CREATE TABLE IF NOT EXISTS BikeStatusHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BikeId INTEGER NOT NULL REFERENCES Bikes(Id),
                OldStatus TEXT NOT NULL,
                NewStatus TEXT NOT NULL,
                StationId INTEGER REFERENCES Stations(Id),
                RentalId INTEGER REFERENCES Rentals(Id),
                ChangedAt TEXT NOT NULL DEFAULT (datetime('now')),
                Note TEXT
            );

            CREATE TABLE IF NOT EXISTS StationStats (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StationId INTEGER NOT NULL REFERENCES Stations(Id),
                Month INTEGER NOT NULL,
                Year INTEGER NOT NULL,
                RentalsStarted INTEGER NOT NULL DEFAULT 0,
                RentalsEnded INTEGER NOT NULL DEFAULT 0
            );
        ");

            SeedData(connection);
        }

        private void SeedData(SqliteConnection connection)
        {
            var stationCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Stations");
            if (stationCount > 0) return;

            connection.Execute(@"
            INSERT INTO Stations (Name, Lat, Lng, Address, Capacity) VALUES
            ('Náměstí Míru', 50.075539, 14.437800, 'Náměstí Míru, Praha 2', 12),
            ('Hlavní nádraží', 50.083100, 14.434900, 'Wilsonova 300, Praha 1', 15),
            ('Anděl', 50.070100, 14.403400, 'Nádražní 1, Praha 5', 10);

            INSERT INTO Bikes (Code, Model, CurrentStationId, Status) VALUES
            ('BIKE-001', 'Trek FX3', 1, 'available'),
            ('BIKE-002', 'Trek FX3', 1, 'available'),
            ('BIKE-003', 'Giant Escape', 2, 'available'),
            ('BIKE-004', 'Giant Escape', 2, 'maintenance'),
            ('BIKE-005', 'Cannondale Quick', 3, 'available');
        ");
        }
    }
}
