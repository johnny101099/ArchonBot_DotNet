using Discord.Commands;
using System.Windows.Input;

namespace ArchonBot.Modules
{
    public class InfoModule(DatabaseContext db, DiscordSocketClient client, ILogger<BaseCommandModule> logger, CommandService commands) : BaseCommandModule(db, client, logger, commands)
    {
        [Command("test")]
        [Summary("在指定頻道（或目前頻道）印出文字")]
        public async Task TestAsync(string text, SocketTextChannel? channel = null)
        {
            var targetChannel = channel ?? Context.Channel as SocketTextChannel;
            if (targetChannel is null)
            {
                await ReplyAsync("❌ 無法找到目標頻道。");
                return;
            }

            await targetChannel.SendMessageAsync(text);
        }

        [Command("info")]
        [Summary("機器人資訊")]
        public async Task InfoAsync()
        {
            var text = "**ArchonBot 資訊**\n" +
                       "- 版本: 1.0.0\n" +
                       "- 開發者: <@394482688297271297>\n" +
                       "- 官方網站: https://archonbot.example.com\n" +
                       "- 支援伺服器數量: " + Bot.Guilds.Count;
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("help")]
        [Summary("顯示訊息指令集")]
        public async Task HelpAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("📘 **指令總覽**");

            foreach (var module in _commands.Modules)
            {
                sb.AppendLine($"\n**{module.Name}**");

                foreach (var cmd in module.Commands)
                {
                    sb.AppendLine($"・`{cmd.Name}` — {cmd.Summary ?? "(無描述)"}");
                }
            }

            await ReplyAsync(sb.ToString());
        }
    }
}
