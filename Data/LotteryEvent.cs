namespace ArchonBot.Data
{
    [Table("LOTTERY_EVENT_MASTER")]
    public class LotteryEvent : LOTTERY_EVENT_MASTER
    {
        public IList<LOTTERY_EVENT_PARTICIPANT> Participants { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> Winners { get; set; } = [];
        public IList<LOTTERY_EVENT_WINNER> ConfirmWinners => Winners.Where(w => w.LEW_HAS_CLAIM).ToList();
        public IList<LOTTERY_EVENT_WINNER> CurrentWinners => Winners.Where(w => w.LEW_BATCH == LEM_DRAW_BATCH).ToList();
        public IList<LOTTERY_EVENT_WINNER> DisplayWinners => Winners.Where(w => w.LEW_HAS_CLAIM || w.LEW_BATCH == LEM_DRAW_BATCH).ToList();


        private bool ReDraw => LEM_DRAW_BATCH > 0;
        private int DrawAmount => LEM_PRIZE_AMOUNT - ConfirmWinners.Count;
        private string AllowDuplicateText => LEM_ALLOW_DUPLICATE ? "是" : "否";
        private string ExcludingPreviousText => LEM_EXCLUDING_PREVIOUS ? "是" : "否";

        private readonly HashSet<string> CanEdit = ["NEW", "OPEN", "DRAW"];

        //  各按鈕依據狀態自動調整
        private ButtonBuilder MainButton
        {
            get {
                var (style, label, custId, isDisabled) = (LEM_STATUS, LEM_DRAW_BATCH) switch
                {
                    ("DRAW", 0) => (ButtonStyle.Success, "抽獎", $"lottery_draw:{Id}", false),
                    ("DRAW", > 0) => (ButtonStyle.Success, "重新抽獎", $"lottery_draw:{Id}", false),
                    ("NEW", _) => (ButtonStyle.Success, "開放參加", $"lottery_toggle:{Id}", false),
                    ("OPEN", _) => (ButtonStyle.Danger, "截止參加", $"lottery_toggle:{Id}", false),
                    ("END", _) => (ButtonStyle.Secondary, "已結束", $"lottery_end:{Id}", true),
                    ("CLOSED", _) => (ButtonStyle.Secondary, "已關閉", $"lottery_closed:{Id}", true),
                    _ => (ButtonStyle.Secondary, "未知狀態", $"lottery_unknown:{Id}", true),
                };
                return new ButtonBuilder(label, custId, style, isDisabled: isDisabled);
            }
        }

        private ButtonBuilder StatusButton
        {
            get
            {
                var label = (LEM_STATUS, LEM_DRAW_BATCH, ConfirmWinners.Count) switch
                {
                    ("DRAW", 0, 0) => $"將抽出: {LEM_PRIZE_NAME} x {LEM_PRIZE_AMOUNT}",
                    ("DRAW", > 0, _) => $"將抽出: {LEM_PRIZE_NAME} x {LEM_PRIZE_AMOUNT - ConfirmWinners.Count}",
                    ("NEW", _, _) => "狀態：新建",
                    ("OPEN", _, _) => $"已於{LEM_START_TIME:YYYY/MM/DD HH:mm:ss}開放參加",
                    ("END", _, _) => "已結束",
                    ("CLOSED", _, _) => "已關閉",
                    _ => "未知狀態",
                };
                return new ButtonBuilder(label, $"lottery_status_info:{Id}", ButtonStyle.Secondary, isDisabled: true);
            }
        }


        public enum UI_MODE
        {
            GENERAL,
            MANAGE,
            PARTICIPATE
        }

        public ComponentBuilderV2 GetComponentBuilderV2(UI_MODE mode = UI_MODE.GENERAL)
        {
            var components = new ComponentBuilderV2([
                new ContainerBuilder()
                    .WithTextDisplay(
                        new TextDisplayBuilder("主要操作：")
                    )
                    .WithActionRow(
                        new ActionRowBuilder([MainButton])
                    )
                    .WithTextDisplay(
                        new TextDisplayBuilder("其他操作：")
                    )
                    .WithActionRow(
                        new ActionRowBuilder()
                            .WithComponents([
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Primary)
                                    .WithLabel("編輯抽獎活動基本資料")
                                    .WithCustomId($"lottery_edit:{Id}"),
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Primary)
                                    .WithLabel("允許重複中獎：否")
                                    .WithCustomId($"lottery_setting:{Id}:allow_duplicate"),
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Primary)
                                    .WithLabel("重抽時排除先前得獎者：是")
                                    .WithCustomId($"lottery_setting:{Id}:excluding_previous"),
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Secondary)
                                    .WithLabel("關閉抽獎活動")
                                    .WithCustomId($"lottery_close:{Id}"),
                            ])
                    )
                    .WithTextDisplay(
                        new TextDisplayBuilder($"-# 此面板將在{DateTime.Now.AddSeconds(60).ToDiscordTimestamp()}失效")
                    )
            ]);
            return components;
        }

        /// <summary>取得管理面板的組件</summary>
        /// <returns>管理面板的ComponentBuilder</returns>
        public ComponentBuilder GetComponentBuilder(UI_MODE mode = UI_MODE.GENERAL)
        {
            var builder = new ComponentBuilder();
            builder.WithButton(MainButton, 0);
            if(LEM_STATUS == "DRAW") {
                builder.WithButton($"將抽出 {LEM_PRIZE_NAME} x {DrawAmount}", $"lottery_draw_amount:{Id}", ButtonStyle.Secondary, disabled: true, row: 0);
            }
            var btn_row = 1;
            if (LEM_STATUS == "DRAW" && ReDraw)
            {
                btn_row = 2;
                builder.WithSelectMenu($"lottery_confirm_winner:{Id}", CurrentWinners.Select(u => new SelectMenuOptionBuilder(u.LEW_USER_NAME, u.Id.ToString())).ToList(), row: 1);
            }
            if (CanEdit.Contains(LEM_STATUS))
            {
                builder.WithButton("編輯抽獎活動基本資料", $"lottery_edit:{Id}", ButtonStyle.Primary, row: btn_row);
                builder.WithButton($"允許重複中獎：{AllowDuplicateText}", $"lottery_setting:{Id}:allow_duplicate", ButtonStyle.Primary, row: btn_row);
                builder.WithButton($"重抽時排除先前得獎者：{ExcludingPreviousText}", $"lottery_setting:{Id}:excluding_previous", ButtonStyle.Primary, row: btn_row);
                builder.WithButton("關閉抽獎活動", $"lottery_close:{Id}", style: ButtonStyle.Secondary, row: btn_row);
            }
            return builder;
        }

        /// <summary>取得參與面板的組件</summary>
        /// <returns>參與面板的ComponentBuilder</returns>
        public ComponentBuilder GetParticipateComponent()
        {
            var builder = new ComponentBuilder();
            builder.WithButton("參加抽獎", $"lottery_join:{Id}", ButtonStyle.Success, row:0);
            builder.WithButton("退出抽獎", $"lottery_leave:{Id}", ButtonStyle.Danger, row: 0);
            builder.WithButton($"當前參與人數: {Participants.Count} 人", $"lottery_participants_count:{Id}", ButtonStyle.Secondary, row: 1);
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
            builder.AddField("備註", string.IsNullOrWhiteSpace(LEM_DESC) ? "無" : LEM_DESC, inline: false);
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
