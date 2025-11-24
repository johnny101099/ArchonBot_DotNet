namespace ArchonBot.Data
{
    public class BotSetting
    {
        /// <summary>機器人令牌</summary>
        public required string Token { get; set; }
        /// <summary>機器人ID</summary>
        public required string Id { get; set; }
        /// <summary>訊息指令前綴碼</summary>
        public required string Prefix { get; set; }
        /// <summary>活動狀態</summary>
        public string? Activity { get; set; }
        public List<ulong> AdminServerIds { get; set; } = [];
        public List<ulong> OwnerIds { get; set; } = [];
        public string? LogChannel { get; set; }
    }
}
