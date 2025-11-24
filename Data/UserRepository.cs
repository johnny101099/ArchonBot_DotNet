using ArchonBot.Models;

namespace ArchonBot.Data
{
    public class UserRepository
    {
        private readonly DatabaseContext _db;

        public UserRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task AddOrUpdateUserAsync(UserRecord user)
        {
            using var conn = _db.GetConnection();
            await conn.ExecuteAsync("""
            INSERT INTO Users (Id, Username, JoinedAt)
            VALUES (@Id, @Username, @JoinedAt)
            ON CONFLICT(Id) DO UPDATE SET Username=@Username, JoinedAt=@JoinedAt;
        """, user);
        }

        public async Task<IEnumerable<UserRecord>> GetAllAsync()
        {
            using var conn = _db.GetConnection();
            return await conn.QueryAsync<UserRecord>("SELECT * FROM Users;");
        }
    }
}
