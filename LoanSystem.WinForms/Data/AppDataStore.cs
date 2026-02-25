using System.Data;
using System.Globalization;
using LoanSystem.WinForms.Domain;
using LoanSystem.WinForms.Security;
using Microsoft.Data.SqlClient;

namespace LoanSystem.WinForms.Data
{
    public class AppDataStore : IAppDataStore
    {
        private readonly string _connectionString;
        private readonly string _masterConnectionString;
        private readonly string _databaseName;
        private readonly object _sync = new();

        public AppDataStore()
        {
            var configuredConnection = Environment.GetEnvironmentVariable("LOANSYSTEM_SQLSERVER_CONNECTION");
            var builder = string.IsNullOrWhiteSpace(configuredConnection)
                ? BuildDefaultConnectionStringBuilder()
                : BuildCustomConnectionStringBuilder(configuredConnection);

            _connectionString = builder.ConnectionString;
            _databaseName = builder.InitialCatalog;

            var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
            {
                InitialCatalog = "master"
            };
            _masterConnectionString = masterBuilder.ConnectionString;

            EnsureDatabaseExistsIfMissing();
            InitializeDatabase();
            SeedAdminIfMissing();
        }

        public List<AppUser> GetUsers()
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, FullName, Email, PasswordHash, Role, Status, CreatedAt
                    FROM dbo.Users
                    ORDER BY CreatedAt DESC;";

                var users = new List<AppUser>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(ReadUser(reader));
                }

                return users;
            }
        }

        public AppUser? GetUserByEmail(string email)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TOP (1) Id, FullName, Email, PasswordHash, Role, Status, CreatedAt
                    FROM dbo.Users
                    WHERE LOWER(Email) = LOWER(@email);";
                command.Parameters.AddWithValue("@email", email.Trim());

                using var reader = command.ExecuteReader();
                return reader.Read() ? ReadUser(reader) : null;
            }
        }

        public AppUser? GetUserById(string userId)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TOP (1) Id, FullName, Email, PasswordHash, Role, Status, CreatedAt
                    FROM dbo.Users
                    WHERE Id = @id;";
                command.Parameters.AddWithValue("@id", userId);

                using var reader = command.ExecuteReader();
                return reader.Read() ? ReadUser(reader) : null;
            }
        }

        public void AddUser(AppUser user)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO dbo.Users (Id, FullName, Email, PasswordHash, Role, Status, CreatedAt)
                    VALUES (@id, @fullName, @email, @passwordHash, @role, @status, @createdAt);";

                BindUserParameters(command, user);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateUser(AppUser user)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE dbo.Users
                    SET FullName = @fullName,
                        Email = @email,
                        PasswordHash = @passwordHash,
                        Role = @role,
                        Status = @status,
                        CreatedAt = @createdAt
                    WHERE Id = @id;";

                BindUserParameters(command, user);
                command.ExecuteNonQuery();
            }
        }

        public List<GameHistory> GetGameHistoryForUser(string userId, int take = 20)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TOP (@take) Id, UserId, GameName, Score, Result, PlayedAt
                    FROM dbo.GameHistories
                    WHERE UserId = @userId
                    ORDER BY PlayedAt DESC;";
                command.Parameters.Add("@take", SqlDbType.Int).Value = take;
                command.Parameters.AddWithValue("@userId", userId);

                var histories = new List<GameHistory>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    histories.Add(ReadGameHistory(reader));
                }

                return histories;
            }
        }

        public List<LoanHistory> GetLoanHistoryForUser(string userId, int take = 20)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TOP (@take) Id, UserId, Amount, InterestRate, TermYears, TermMonths, TotalPayment, CreatedAt
                    FROM dbo.LoanHistories
                    WHERE UserId = @userId
                    ORDER BY CreatedAt DESC;";
                command.Parameters.Add("@take", SqlDbType.Int).Value = take;
                command.Parameters.AddWithValue("@userId", userId);

                var histories = new List<LoanHistory>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    histories.Add(ReadLoanHistory(reader));
                }

                return histories;
            }
        }

        public void AddGameHistory(GameHistory history)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO dbo.GameHistories (UserId, GameName, Score, Result, PlayedAt)
                    VALUES (@userId, @gameName, @score, @result, @playedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                command.Parameters.AddWithValue("@userId", history.UserId);
                command.Parameters.AddWithValue("@gameName", history.GameName);
                command.Parameters.AddWithValue("@score", history.Score);
                command.Parameters.AddWithValue("@result", history.Result);
                command.Parameters.AddWithValue("@playedAt", history.PlayedAt);

                var insertedId = command.ExecuteScalar();
                if (insertedId != null && insertedId != DBNull.Value)
                {
                    history.Id = Convert.ToInt32(insertedId, CultureInfo.InvariantCulture);
                }
            }
        }

        public void AddLoanHistory(LoanHistory history)
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO dbo.LoanHistories (UserId, Amount, InterestRate, TermYears, TermMonths, TotalPayment, CreatedAt)
                    VALUES (@userId, @amount, @interestRate, @termYears, @termMonths, @totalPayment, @createdAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                command.Parameters.AddWithValue("@userId", history.UserId);
                command.Parameters.AddWithValue("@amount", history.Amount);
                command.Parameters.AddWithValue("@interestRate", history.InterestRate);
                command.Parameters.AddWithValue("@termYears", history.TermYears);
                command.Parameters.AddWithValue("@termMonths", history.TermMonths);
                command.Parameters.AddWithValue("@totalPayment", history.TotalPayment);
                command.Parameters.AddWithValue("@createdAt", history.CreatedAt);

                var insertedId = command.ExecuteScalar();
                if (insertedId != null && insertedId != DBNull.Value)
                {
                    history.Id = Convert.ToInt32(insertedId, CultureInfo.InvariantCulture);
                }
            }
        }

        private void EnsureDatabaseExists()
        {
            lock (_sync)
            {
                using var connection = new SqlConnection(_masterConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                var safeDatabaseName = _databaseName.Replace("]", "]]", StringComparison.Ordinal);
                command.CommandText = $@"
                    IF DB_ID(@databaseName) IS NULL
                    BEGIN
                        CREATE DATABASE [{safeDatabaseName}];
                    END;";
                command.Parameters.Add("@databaseName", SqlDbType.NVarChar, 128).Value = _databaseName;
                command.ExecuteNonQuery();
            }
        }

        private void EnsureDatabaseExistsIfMissing()
        {
            lock (_sync)
            {
                try
                {
                    using var connection = OpenConnection();
                    return;
                }
                catch (SqlException ex) when (IsMissingDatabaseError(ex))
                {
                    EnsureDatabaseExists();
                }
            }
        }

        private void InitializeDatabase()
        {
            lock (_sync)
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Users (
                            Id NVARCHAR(64) NOT NULL PRIMARY KEY,
                            FullName NVARCHAR(200) NOT NULL,
                            Email NVARCHAR(320) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(512) NOT NULL,
                            Role INT NOT NULL,
                            Status INT NOT NULL,
                            CreatedAt DATETIME2(7) NOT NULL
                        );
                    END;

                    IF OBJECT_ID(N'dbo.GameHistories', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.GameHistories (
                            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            UserId NVARCHAR(64) NOT NULL,
                            GameName NVARCHAR(100) NOT NULL,
                            Score INT NOT NULL,
                            Result NVARCHAR(200) NOT NULL,
                            PlayedAt DATETIME2(7) NOT NULL,
                            CONSTRAINT FK_GameHistories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                        );
                    END;

                    IF OBJECT_ID(N'dbo.LoanHistories', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.LoanHistories (
                            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            UserId NVARCHAR(64) NOT NULL,
                            Amount DECIMAL(18,2) NOT NULL,
                            InterestRate FLOAT NOT NULL,
                            TermYears INT NOT NULL,
                            TermMonths INT NOT NULL,
                            TotalPayment DECIMAL(18,2) NOT NULL,
                            CreatedAt DATETIME2(7) NOT NULL,
                            CONSTRAINT FK_LoanHistories_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                        );
                    END;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_GameHistories_UserId_PlayedAt'
                          AND object_id = OBJECT_ID(N'dbo.GameHistories')
                    )
                    BEGIN
                        CREATE INDEX IX_GameHistories_UserId_PlayedAt
                        ON dbo.GameHistories(UserId, PlayedAt DESC);
                    END;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_LoanHistories_UserId_CreatedAt'
                          AND object_id = OBJECT_ID(N'dbo.LoanHistories')
                    )
                    BEGIN
                        CREATE INDEX IX_LoanHistories_UserId_CreatedAt
                        ON dbo.LoanHistories(UserId, CreatedAt DESC);
                    END;";
                command.ExecuteNonQuery();
            }
        }

        private void SeedAdminIfMissing()
        {
            lock (_sync)
            {
                const string adminEmail = "admin@loansystem.com";
                using var connection = OpenConnection();
                using var existsCommand = connection.CreateCommand();
                existsCommand.CommandText = @"
                    SELECT COUNT(1)
                    FROM dbo.Users
                    WHERE LOWER(Email) = LOWER(@email);";
                existsCommand.Parameters.AddWithValue("@email", adminEmail);

                var existingCount = Convert.ToInt32(existsCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
                if (existingCount > 0)
                {
                    return;
                }

                var adminUser = new AppUser
                {
                    FullName = "System Administrator",
                    Email = adminEmail,
                    PasswordHash = PasswordHashing.Hash("Admin123!"),
                    Role = UserRole.Admin,
                    Status = AccountStatus.Active
                };

                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"
                    INSERT INTO dbo.Users (Id, FullName, Email, PasswordHash, Role, Status, CreatedAt)
                    VALUES (@id, @fullName, @email, @passwordHash, @role, @status, @createdAt);";
                BindUserParameters(insertCommand, adminUser);
                insertCommand.ExecuteNonQuery();
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static bool IsMissingDatabaseError(SqlException ex)
        {
            return ex.Number == 4060;
        }

        private static SqlConnectionStringBuilder BuildDefaultConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = "LoanSystemWinForms",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                Encrypt = false
            };
        }

        private static SqlConnectionStringBuilder BuildCustomConnectionStringBuilder(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString.Trim());
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                throw new InvalidOperationException(
                    "LOANSYSTEM_SQLSERVER_CONNECTION must include an Initial Catalog/Database name.");
            }

            return builder;
        }

        private static void BindUserParameters(SqlCommand command, AppUser user)
        {
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@fullName", user.FullName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@role", (int)user.Role);
            command.Parameters.AddWithValue("@status", (int)user.Status);
            command.Parameters.AddWithValue("@createdAt", user.CreatedAt);
        }

        private static AppUser ReadUser(SqlDataReader reader)
        {
            return new AppUser
            {
                Id = reader.GetString(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = (UserRole)reader.GetInt32(4),
                Status = (AccountStatus)reader.GetInt32(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }

        private static GameHistory ReadGameHistory(SqlDataReader reader)
        {
            return new GameHistory
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetString(1),
                GameName = reader.GetString(2),
                Score = reader.GetInt32(3),
                Result = reader.GetString(4),
                PlayedAt = reader.GetDateTime(5)
            };
        }

        private static LoanHistory ReadLoanHistory(SqlDataReader reader)
        {
            return new LoanHistory
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetString(1),
                Amount = reader.GetDecimal(2),
                InterestRate = reader.GetDouble(3),
                TermYears = reader.GetInt32(4),
                TermMonths = reader.GetInt32(5),
                TotalPayment = reader.GetDecimal(6),
                CreatedAt = reader.GetDateTime(7)
            };
        }
    }
}
