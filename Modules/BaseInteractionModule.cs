using Discord.Interactions;

namespace ArchonBot.Modules
{
    /// <summary>應用指令（Slash）基底模組，所有應用指令模組都應繼承此類別。</summary>
    public abstract class BaseInteractionModule(
        InteractionService interactions,
        BotService bot,
        DatabaseContext dbContext,
        AdminService adminService,
        ILogger<BaseInteractionModule> logger) : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>互動服務</summary>
        protected readonly InteractionService _interactions = interactions;
        /// <summary>Bot服務</summary>
        protected readonly BotService Bot = bot;
        /// <summary>資料庫工具</summary>
        protected readonly DatabaseContext DbContext = dbContext;
        /// <summary>系統管理服務</summary>
        protected readonly AdminService AdminService = adminService;
        /// <summary>Logger</summary>
        protected readonly ILogger Logger = logger;
    }
}
