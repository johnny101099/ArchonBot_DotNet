using Discord.Commands;

namespace ArchonBot.Modules
{
    /// <summary>
    /// 訊息指令基底模組，所有文字指令模組都應繼承此類別。
    /// </summary>
    public abstract class BaseCommandModule : ModuleBase<SocketCommandContext>
    {
        protected readonly DatabaseContext DbContext;
        protected readonly DiscordSocketClient Bot;
        protected readonly CommandService _commands;
        protected readonly ILogger Logger;

        public BaseCommandModule(DatabaseContext db, DiscordSocketClient client, ILogger<BaseCommandModule> logger, CommandService commands)
        {
            DbContext = db;
            Bot = client;
            Logger = logger;
            _commands = commands;
        }
    }
}
