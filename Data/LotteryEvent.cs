namespace ArchonBot.Data
{
    [Table("LOTTERY_EVENT_MASTER")]
    public class LotteryEvent : LOTTERY_EVENT_MASTER
    {
        public IList<LOTTERY_EVENT_PARTICIPANT> Participants { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> Winners { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> ConfirmWinners => Winners.Where(w => w.LEW_HAS_CLAIM).ToList();

        public ComponentBuilder GetComponentBuilder()
        {
            var CanOpen = LEM_STATUS == "NEW";
            var toggleLabel = CanOpen ? "開放抽獎" : "截止抽獎";
            var toggleStyle = CanOpen ? ButtonStyle.Success : ButtonStyle.Danger;
            var builder = new ComponentBuilder();
            if(LEM_STATUS == "NEW" || LEM_STATUS == "OPEN")
            {
                builder.WithButton(label: toggleLabel, customId: $"lottery_toggle:{Id}", style: toggleStyle);
            }
            builder.WithButton(label: "編輯抽獎設定", customId: $"lottery_edit:{Id}", style: ButtonStyle.Primary);
            builder.WithButton(label: "關閉活動", customId: $"lottery_close:{Id}", style: ButtonStyle.Secondary);
            return builder;
        }

        public async Task<ComponentBuilder> GetParticipateComponent(DiscordSocketClient client)
        {
            var builder = new ComponentBuilder();
            builder.WithButton(label: "參加抽獎", customId: $"lottery_join:{Id}", style: ButtonStyle.Success);
            builder.WithButton(label: "退出抽獎", customId: $"lottery_leave:{Id}", style: ButtonStyle.Danger);
            return builder;
        }

        public async Task<EmbedBuilder> GetParticipateEmbed(DiscordSocketClient client)
        {
            var guild = client.GetGuild(LEM_GUILD_ID.Value);
            var owner = await client.GetUserAsync(LEM_OWNER_ID);
            var builder = new EmbedBuilder().WithTitle(LEM_NAME);
            var desc = new List<string>
            {
                $"**預計抽出 `{LEM_PRIZE_AMOUNT}` 個獎項**",
                $"開放時間：{LEM_CREATE_TIME.ToDiscordTimestamp(DiscordTimestampFormat.LongDateTime)}"
            };
            if(LEM_STATUS == "DRAW")
            {
                desc.Add($"截止時間：{LEM_END_TIME.ToDiscordTimestamp(DiscordTimestampFormat.LongDateTime)}");
            }
            desc.Add($"參與人數：`{Participants.Count}`人");
            builder.WithDescription(">>> " + string.Join("\n", desc));
            builder.AddField("伺服器", guild.Name, inline: true);
            builder.AddField("建立者", owner.Mention, inline: true);
            builder.AddField("備註", LEM_DESC, inline: false);
            builder.WithThumbnailUrl(owner.GetDisplayAvatarUrl());
            return builder;
        }



        public async Task<EmbedBuilder> GetEmbedBuilder(DiscordSocketClient client, bool manager = false)
        {
            var owner = await client.GetUserAsync(LEM_OWNER_ID);
            var builder = new EmbedBuilder().WithTitle(LEM_NAME);
            var desc = new List<string>
            {
                $"建立者　：{owner.Mention}",
                $"建立時間：{LEM_CREATE_TIME.ToDiscordTimestamp()}",
                $"重複中獎：`{(LEM_ALLOW_DUPLICATE ? "允許" : "不允許")}`",
                $"參與人數：`{Participants.Count}`人",
            };
            if (LEM_STATUS == "DRAW")
            {
                desc.Add($"開放時間：{LEM_START_TIME.ToDiscordTimestamp()} ~ {LEM_END_TIME.ToDiscordTimestamp()}");
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個，`{ConfirmWinners.Count}`個已領獎");
            }
            if (LEM_STATUS == "CLOSED")
            {
                desc.Add($"結束時間：{LEM_CLOSE_TIME.ToDiscordTimestamp()}");
            }
            builder.WithDescription(string.Join("\n", desc));
            builder.WithColor(Color.Blue);
            return builder;
        }
    }
}
