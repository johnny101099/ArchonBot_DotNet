namespace ArchonBot.Models
{
    public class UserRecord
    {
        public ulong Id { get; set; } // Discord User ID
        public string Username { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
