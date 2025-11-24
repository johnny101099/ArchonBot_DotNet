using Discord.Interactions;

namespace ArchonBot.Modules
{
    internal class BasicInteractionModule(
        InteractionService interactions,
        BotService bot,
        DatabaseContext dbContext,
        AdminService adminService,
        ILogger<BasicInteractionModule> logger) 
        : BaseInteractionModule(interactions, bot, dbContext, adminService, logger)
    {
        [SlashCommand("help", "顯示所有 Slash 指令")]
        public async Task HelpSlashAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("📘 **Slash 指令總覽**");

            foreach (var module in _interactions.Modules)
            {
                sb.AppendLine($"\n**{module.Name}**");

                foreach (var cmd in module.SlashCommands)
                {
                    sb.AppendLine($"・`/{cmd.Name}` — {cmd.Description ?? "無描述"}");
                }
            }

            await RespondAsync(sb.ToString(), ephemeral: true);
        }
    }
}
