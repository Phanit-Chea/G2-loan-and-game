using System.Text.Json;
using LoanSystem.WinForms.Domain;
using LoanSystem.WinForms.Security;

namespace LoanSystem.WinForms.Data
{
    public class AppDataStore : IAppDataStore
    {
        private readonly string _dataFilePath;
        private readonly object _sync = new();
        private AppData _data = new();

        public AppDataStore()
        {
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LoanSystemWinForms");
            Directory.CreateDirectory(root);
            _dataFilePath = Path.Combine(root, "data.json");
            Load();
            SeedAdminIfMissing();
        }

        public List<AppUser> GetUsers()
        {
            lock (_sync)
            {
                return _data.Users.Select(x => CloneUser(x)).ToList();
            }
        }

        public AppUser? GetUserByEmail(string email)
        {
            lock (_sync)
            {
                var user = _data.Users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                return user == null ? null : CloneUser(user);
            }
        }

        public AppUser? GetUserById(string userId)
        {
            lock (_sync)
            {
                var user = _data.Users.FirstOrDefault(x => x.Id == userId);
                return user == null ? null : CloneUser(user);
            }
        }

        public void AddUser(AppUser user)
        {
            lock (_sync)
            {
                _data.Users.Add(CloneUser(user));
                Save();
            }
        }

        public void UpdateUser(AppUser user)
        {
            lock (_sync)
            {
                var index = _data.Users.FindIndex(x => x.Id == user.Id);
                if (index < 0)
                {
                    return;
                }

                _data.Users[index] = CloneUser(user);
                Save();
            }
        }

        public List<GameHistory> GetGameHistoryForUser(string userId, int take = 20)
        {
            lock (_sync)
            {
                return _data.GameHistories
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.PlayedAt)
                    .Take(take)
                    .Select(CloneGameHistory)
                    .ToList();
            }
        }

        public List<LoanHistory> GetLoanHistoryForUser(string userId, int take = 20)
        {
            lock (_sync)
            {
                return _data.LoanHistories
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(take)
                    .Select(CloneLoanHistory)
                    .ToList();
            }
        }

        public void AddGameHistory(GameHistory history)
        {
            lock (_sync)
            {
                history.Id = _data.NextGameHistoryId++;
                _data.GameHistories.Add(CloneGameHistory(history));
                Save();
            }
        }

        public void AddLoanHistory(LoanHistory history)
        {
            lock (_sync)
            {
                history.Id = _data.NextLoanHistoryId++;
                _data.LoanHistories.Add(CloneLoanHistory(history));
                Save();
            }
        }

        private void Load()
        {
            lock (_sync)
            {
                if (!File.Exists(_dataFilePath))
                {
                    _data = new AppData();
                    return;
                }

                var json = File.ReadAllText(_dataFilePath);
                _data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_dataFilePath, json);
        }

        private void SeedAdminIfMissing()
        {
            lock (_sync)
            {
                var adminEmail = "admin@loansystem.com";
                if (_data.Users.Any(x => x.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                _data.Users.Add(new AppUser
                {
                    FullName = "System Administrator",
                    Email = adminEmail,
                    PasswordHash = PasswordHashing.Hash("Admin123!"),
                    Role = UserRole.Admin,
                    Status = AccountStatus.Active
                });

                Save();
            }
        }

        private static AppUser CloneUser(AppUser user)
        {
            return new AppUser
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }

        private static GameHistory CloneGameHistory(GameHistory history)
        {
            return new GameHistory
            {
                Id = history.Id,
                UserId = history.UserId,
                GameName = history.GameName,
                Score = history.Score,
                Result = history.Result,
                PlayedAt = history.PlayedAt
            };
        }

        private static LoanHistory CloneLoanHistory(LoanHistory history)
        {
            return new LoanHistory
            {
                Id = history.Id,
                UserId = history.UserId,
                Amount = history.Amount,
                InterestRate = history.InterestRate,
                TermYears = history.TermYears,
                TermMonths = history.TermMonths,
                TotalPayment = history.TotalPayment,
                CreatedAt = history.CreatedAt
            };
        }
    }
}
