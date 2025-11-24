namespace ArchonBot.Services
{
    public class AdminService
    {
        private readonly BotSetting _config;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<BotService> _logger;
        public AdminService(
            IOptions<BotSetting> config,
            DatabaseContext dbContext,
            ILogger<BotService> logger)
        {
            _config = config.Value;
            _dbContext = dbContext;
            _logger = logger;
        }

        public string GetBotId()
        {
            return _config.Id;
        }

        public IEnumerable<BOT_PARAM_DETAIL> GetParamDetails(string moduleId, string type, ulong? guildId = null)
        {
            var sql = "" +
                " SELECT * FROM BOT_PARAM_DETAIL " +
                " WHERE BPD_BOT_ID = @BotId " +
                "   AND BPD_MODULE_ID = @ModId " +
                "   AND BPD_PARAM_TYPE = @ParamType " +
                "   AND BPD_GUILD_ID = @GuildId ";
            var param = new DynamicParameters();
            param.Add("ModId", moduleId);
            param.Add("ParamType", type);
            param.Add("BotId", _config.Id);
            param.Add("GuildId", guildId.HasValue ? (long)guildId.Value : -1);
            return _dbContext.Query<BOT_PARAM_DETAIL>(sql, param);
        }

        public BOT_PARAM_DETAIL? GetParamDetail(string moduleId, string type, string paramId, ulong? guildId = null)
        {
            var sql = "" +
                " SELECT * FROM BOT_PARAM_DETAIL " +
                " WHERE BPD_BOT_ID = @BotId " +
                "   AND BPD_MODULE_ID = @ModId " +
                "   AND BPD_PARAM_TYPE = @ParamType " +
                "   AND BPD_PARAM_ID = @ParamId " +
                "   AND BPD_GUILD_ID = @GuildId ";
            var param = new DynamicParameters();
            param.Add("BotId", _config.Id);
            param.Add("ModId", moduleId);
            param.Add("ParamType", type);
            param.Add("ParamId", type);
            param.Add("GuildId", guildId.HasValue ? (long)guildId.Value : -1);
            return _dbContext.QueryFirstOrDefault<BOT_PARAM_DETAIL>(sql, param);
        }

        public ResultInfo SetParamDetail(BOT_PARAM_DETAIL data)
        {
            var columns = new List<string>
            {
                "BPD_PARAM_VALUE",
                "BPD_PARAM_NAME",
                "BPD_PARAM_ATTR",
                "BPD_PARAM_DESC"
            };
            var param = data.ToDictionary(columns);
            var (Success, Message) = _dbContext.RunTx(() =>
            {
                _dbContext.Update<BOT_PARAM_DETAIL>(data.Id, param);
            });
            return new ResultInfo { Success = Success, Message = Message };
        }
    }
}
