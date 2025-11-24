using Discord.Interactions;
using Discord.Net;

namespace ArchonBot.Modules
{
    public class TextModule(InteractionService interactions, BotService bot,
                            DatabaseContext dbContext, AdminService adminService, ILogger<TextModule> logger) 
                            : BaseInteractionModule(interactions, bot, dbContext, adminService, logger)
    {
        [SlashCommand("say", "讓機器人在指定頻道（或當前頻道）說話")]
        public async Task SayAsync(string message, ITextChannel? channel = null)
        {
            await DeferAsync();
            var TargetChannel = channel == null ? Context.Channel : (ISocketMessageChannel)channel;
            IUserMessage msg = await TargetChannel.SendMessageAsync(message);
            Logger.LogInformation($"於 # {TargetChannel.Name} 發送訊息。");
            await FollowupAsync($"✅ 已發送訊息 : {msg.GetJumpUrl()}", ephemeral: true);
        }

        [SlashCommand("reply", "回覆指定訊息")]
        public async Task ReplyAsync(
            [Summary("訊息內容", "回覆的訊息內容")] string message,
            [Summary("目標訊息", "要回覆的訊息目標(接受ID或連結)")] string TargetMessageId)
        {
            await DeferAsync();
            //try
            //{
                ParseMessageId(TargetMessageId, out var channelId, out var messageId);
                var channel = await Context.Client.GetChannelAsync(channelId ?? Context.Channel.Id) as IMessageChannel;
                if (channel == null)
                {
                    await FollowupAsync("找不到訊息所在的頻道", ephemeral: true);
                    return;
                }
                var TargetMessage = await channel.GetMessageAsync(messageId) as IUserMessage;
                if (TargetMessage == null)
                {
                    await FollowupAsync("找不到要回覆的訊息", ephemeral: true);
                    return;
                }

                var msg = await TargetMessage.ReplyAsync(message);
                await FollowupAsync($"✅ 已回覆訊息:{msg.GetJumpUrl()}", ephemeral: true);
            //}
            //catch (HttpException http)
            //{
            //    Logger.LogError($"HTTP Error: {http.HttpCode}, DiscordCode: {http.DiscordCode}, {http.Message}");
            //    await FollowupAsync("發生錯誤，請稍後再試", ephemeral: true);
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex.GetType().Name);
            //    Logger.LogError(ex.Message);
            //    Logger.LogError(ex.StackTrace);
            //    await FollowupAsync("發生錯誤，請稍後再試", ephemeral: true);
            //}
        }
        private void ParseMessageId(string messageInput, out ulong? channelId, out ulong messageId)
        {
            if (messageInput.StartsWith("http"))
            {
                var parts = messageInput.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 3)
                    throw new FormatException("訊息 URL 格式錯誤");

                // 最後三段一定是 guild / channel / message
                ulong guildId = ulong.Parse(parts[^3]);    // 可用可不用
                channelId = ulong.Parse(parts[^2]);
                messageId = ulong.Parse(parts[^1]);
                return;
            }
            if (messageInput.Contains('-'))
            {
                var parts = messageInput.Split('-');
                if (parts.Length != 2)
                    throw new FormatException("不應有超過一個以上的-");

                channelId = ulong.Parse(parts[0]);
                messageId = ulong.Parse(parts[1]);
                return;
            }
            // 若只有訊息ID，使用當前頻道
            channelId = null;
            messageId = ulong.Parse(messageInput);
        }

        //[SlashCommand("reply", "回覆指定訊息")]
        //public async Task ReplyAsync(string message, IMessage targetMessage)
        //{

        //    await (targetMessage.Channel as ITextChannel)!.SendMessageAsync($"↩️ 回覆：{message}", messageReference: new MessageReference(targetMessage.Id));
        //    await RespondAsync("✅ 已回覆訊息", ephemeral: true);
        //}

        //[SlashCommand("edit", "編輯機器人發送的訊息")]
        //public async Task EditAsync(string newText, IMessage targetMessage)
        //{
        //    if (targetMessage.Author.Id != Context.Client.CurrentUser.Id)
        //    {
        //        await RespondAsync("⚠️ 只能編輯機器人自己發送的訊息。", ephemeral: true);
        //        return;
        //    }

        //    await (targetMessage as IUserMessage)!.ModifyAsync(m => m.Content = newText);
        //    await RespondAsync("✅ 訊息已更新", ephemeral: true);
        //}

        //[SlashCommand("delete", "刪除機器人發送的訊息")]
        //public async Task DeleteAsync(IMessage targetMessage)
        //{
        //    if (targetMessage.Author.Id != Context.Client.CurrentUser.Id)
        //    {
        //        await RespondAsync("⚠️ 只能刪除機器人自己發送的訊息。", ephemeral: true);
        //        return;
        //    }

        //    await targetMessage.DeleteAsync();
        //    await RespondAsync("🗑️ 訊息已刪除", ephemeral: true);
        //}

    }
}