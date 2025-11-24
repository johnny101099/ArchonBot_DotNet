namespace ArchonBot.Data
{
    [Table("LOTTERY_EVENT_MASTER")]
    public class LotteryEvent : LOTTERY_EVENT_MASTER
    {
        public IList<LOTTERY_EVENT_PARTICIPANT> Participants { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> Winners { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> ConfirmWinners => Winners.Where(w => w.LEW_HAS_CLAIM).ToList();

        /// <summary>取得管理面板的組件</summary>
        /// <returns>管理面板的ComponentBuilder</returns>
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
            else if(LEM_STATUS == "DRAW")
            {
                var label = LEM_DRAW_BATCH == 0 ? "抽獎" : "重新抽獎";
                builder.WithButton(label: label, customId: $"lottery_draw:{Id}", style: ButtonStyle.Success);
            }
            builder.WithButton(label: "編輯抽獎設定", customId: $"lottery_edit:{Id}", style: ButtonStyle.Primary);
            builder.WithButton(label: "關閉活動", customId: $"lottery_close:{Id}", style: ButtonStyle.Secondary);
            return builder;
        }

        /// <summary>取得參與面板的組件</summary>
        /// <returns>參與面板的ComponentBuilder</returns>
        public ComponentBuilder GetParticipateComponent()
        {
            var builder = new ComponentBuilder();
            builder.WithButton(label: "參加抽獎", customId: $"lottery_join:{Id}", style: ButtonStyle.Success);
            builder.WithButton(label: "退出抽獎", customId: $"lottery_leave:{Id}", style: ButtonStyle.Danger);
            return builder;
        }

        /// <summary>取得抽獎活動的參與面板</summary>
        /// <param name="guild">舉辦活動之伺服器</param>
        /// <param name="owner">抽獎活動的建立者</param>
        /// <returns>參與面板的EmbedBuilder</returns>
        public EmbedBuilder GetParticipateEmbed(IGuild guild, IUser owner)
        {
            var builder = new EmbedBuilder().WithTitle(LEM_NAME);
            var desc = new List<string>
            {
                $"**預計抽出 `{LEM_PRIZE_AMOUNT}` 個獎項**",
                $"開放時間：{LEM_CREATE_TIME.ToDiscordTimestamp(DiscordTimestampFormat.ShortDateTime)}"
            };
            if(LEM_STATUS == "DRAW")
            {
                desc.Add($"截止時間：{LEM_END_TIME.ToDiscordTimestamp(DiscordTimestampFormat.ShortDateTime)}");
            }
            desc.Add($"參與人數：`{Participants.Count}`人");
            builder.WithDescription(">>> " + string.Join("\n", desc));
            builder.AddField("伺服器", guild.Name, inline: true);
            builder.AddField("建立者", owner.Mention, inline: true);
            builder.AddField("備註", LEM_DESC, inline: false);
            builder.WithThumbnailUrl(owner.GetDisplayAvatarUrl());
            return builder;
        }

        /// <summary>抽獎並取得抽獎結果面板</summary>
        /// <returns>抽獎結果面板的EmbedBuilder</returns>
        public EmbedBuilder DrawAndGetResultEmbed()
        {
            var builder = new EmbedBuilder().WithTitle(LEM_NAME);
            return builder;
        }


        /// <summary>取得抽獎事件的管理狀態面板</summary>
        /// <param name="client">機器人客戶端，用於取得相關資料</param>
        /// <returns>管理面板的EmbedBuilder</returns>
        public async Task<EmbedBuilder> GetEmbedBuilder(DiscordSocketClient client)
        {
            var owner = await client.GetUserAsync(LEM_OWNER_ID);
            var builder = new EmbedBuilder().WithTitle(LEM_NAME);
            var desc = new List<string>
            {
                $"建立者　：{owner.Mention}",
                $"建立時間：{LEM_CREATE_TIME.ToDiscordTimestamp()}",
                $"建立時間：{LEM_CREATE_TIME.ToDiscordTimestamp()}",
                $"重複中獎：`{(LEM_ALLOW_DUPLICATE ? "允許" : "不允許")}`"
            };
            if (LEM_STATUS == "NEW")
            {
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個");
            }
            if (LEM_STATUS == "OPEN")
            {
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個");
                desc.Add($"開放時間：{LEM_START_TIME.ToDiscordTimestamp()} ~ (尚未結束)");
                desc.Add($"參與人數：`{Participants.Count}`人");
            }
            if (LEM_STATUS == "DRAW")
            {
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個，`{ConfirmWinners.Count}`個已領獎");
                desc.Add($"開放時間：{LEM_START_TIME.ToDiscordTimestamp()} ~ {LEM_END_TIME.ToDiscordTimestamp()}");
                desc.Add($"參與人數：`{Participants.Count}`人");
            }
            if (LEM_STATUS == "END")
            {
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個，`{ConfirmWinners.Count}`個已領獎");
                desc.Add($"開放時間：{LEM_START_TIME.ToDiscordTimestamp()} ~ {LEM_END_TIME.ToDiscordTimestamp()}");
                desc.Add($"參與人數：`{Participants.Count}`人");
                desc.Add($"結束時間：{LEM_CLOSE_TIME.ToDiscordTimestamp()}");
            }
            if (LEM_STATUS == "CLOSED")
            {
                desc.Add($"抽獎數量：`{LEM_PRIZE_AMOUNT}`個，`{ConfirmWinners.Count}`個已領獎");
                desc.Add($"開放時間：{LEM_START_TIME.ToDiscordTimestamp()} ~ {LEM_END_TIME.ToDiscordTimestamp()}");
                desc.Add($"參與人數：`{Participants.Count}`人");
                desc.Add($"結束時間：{LEM_CLOSE_TIME.ToDiscordTimestamp()}");
            }

            builder.WithDescription(string.Join("\n", desc));
            builder.WithColor(Color.Blue);
            return builder;
        }
    }
}
