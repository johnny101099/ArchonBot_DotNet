using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ArchonBot.Modules
{
    public class LotteryModule(
        InteractionService interactions,
        BotService bot,
        DatabaseContext dbContext,
        AdminService adminService,
        ILogger<LotteryModule> logger
        ) :　BaseInteractionModule(interactions, bot, dbContext, adminService, logger)
    {
        //lottery create
        [SlashCommand("lottery_create", "建立新的抽獎活動")]
        public async Task CreateLotteryAsync()
        {
            var modal = new ModalBuilder("建立抽獎", "lottery_edit_modal:0")
                .AddTextInput("抽獎名稱（必填）", "lottery_name", placeholder: "例如：歐了一把，抽2張月卡", required: true)
                .AddTextInput("獎品數（必填）", "lottery_count", placeholder: "需為正整數，例：2", value: "1", required: true)
                .AddTextInput("預計抽獎時間（選填）", "lottery_time", placeholder: "例：2025-08-31 20:25:39", required: false)
                .AddTextInput("抽獎說明（選填）", "lottery_desc", TextInputStyle.Paragraph, placeholder: "其他關於本活動的備註，像是特殊規則或說明", required: false);
            
            await RespondWithModalAsync(modal.Build());
        }

        public class LotterEventModal : IModal
        {
            public string Title => "建立抽獎";

            [RequiredInput(true)]
            [InputLabel("抽獎名稱（必填）")]
            [ModalTextInput("lottery_name", placeholder: "例如：歐了一把，抽2張月卡", maxLength: 50)]
            public string? Name { get; set; }

            [RequiredInput(true)]
            [InputLabel("獎品數（必填）")]
            [ModalTextInput("lottery_count", placeholder: "需為正整數，例：2", maxLength: 3, initValue: "1")]
            public string? PrizeAmount { get; set; }

            [RequiredInput(false)]
            [InputLabel("預計抽獎時間（選填）")]
            [ModalTextInput("lottery_time", placeholder: "例：2025-08-31 20:25:39", maxLength: 3, initValue: "1")]
            public string? ExpectedTime { get; set; }

            [RequiredInput(false)]
            [InputLabel("抽獎說明（選填）")]
            [ModalTextInput("lottery_desc", TextInputStyle.Paragraph, "其他關於本活動的備註，像是特殊規則或說明", maxLength: 500)]
            public string? Description { get; set; }
        }

        [ModalInteraction("lottery_edit_modal:*")]
        public async Task HandleEventModalAsync(string lottery_id, LotterEventModal modal)
        {
            await DeferAsync(ephemeral: true);
            var id = long.Parse(lottery_id);
            var Lottery = DbContext.Get<LOTTERY_EVENT_MASTER>(long.Parse(lottery_id));
            var newCreate = id == 0 || Lottery == null;
            var operation = newCreate ? "建立" : "編輯";
            Logger.LogDebug($"Modal submitted: {modal.Title}");
            Logger.LogDebug($"LEM_SEQ: {Lottery?.LEM_SEQ}，NewCreate: {Lottery?.NewCreate}");

            if (!int.TryParse(modal.PrizeAmount, out int count))
            {
                await FollowupAsync("❌ **獎品數格式錯誤，請輸入正整數！**", ephemeral: true);
                return;
            }
            if (count <= 0)
            {
                await FollowupAsync("❌ **獎品數必須是正整數，請勿輸入負數！**", ephemeral: true);
                return;
            }

            // 🧪 驗證時間
            DateTime? drawTime = null;
            if (!string.IsNullOrWhiteSpace(modal.ExpectedTime))
            {
                if (!DateTime.TryParseExact(modal.ExpectedTime, "yyyy-MM-dd HH:mm:ss", null,
                    System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    await FollowupAsync("❌ **時間格式錯誤：請使用 `YYYY-MM-DD HH:mm:ss` 的格式進行填寫**", ephemeral: true);
                    return;
                }

                drawTime = parsed;
            }
            var dict = new Dictionary<string, object?>
            {
                {"LEM_NAME", modal.Name},
                {"LEM_PRIZE_AMOUNT", count},
                {"LEM_DESC", modal.Description},
                {"LEM_DRAW_TIME", drawTime},
            };
            if (newCreate)
            {
                dict.Add("LEM_OWNER_ID", Context.User.Id);
            }
            // 🗄️ 寫入資料庫（你可依你的資料表調整）
            var (Success, Message) = DbContext.RunTx(() =>
            {
                if (newCreate)
                {
                    id = DbContext.Insert<LOTTERY_EVENT_MASTER>(dict);
                }
                else
                {
                    DbContext.Update<LOTTERY_EVENT_MASTER>(id, dict);
                }
            });

            if (!Success)
            {
                Logger.LogWarning($"{Context.User.Username} {operation}抽獎活動失敗：{Message}");
                await FollowupAsync($"❌ **抽獎活動{operation}失敗：{Message}**", ephemeral: true);
                return;
            }
            await FollowupAsync($"🎉 **抽獎活動{operation}成功！**\n活動名稱：`{modal.Name}`\n活動編號：`{id}`", ephemeral: true);
        }


        //lottery create
        [SlashCommand("lottery_manager", "管理抽獎活動")]
        public async Task ManagerLotteryAsync(
            [Autocomplete(typeof(LotteryIdAutocomplete))]
            [Summary("抽獎活動", "要管理的抽獎活動")] long lottery_id)
        {
            await DeferAsync(ephemeral: true);
            Logger.LogDebug("選擇的抽獎活動ID：" + lottery_id);
            var Lottery = GetLotteryEvent(lottery_id);
            if (Lottery == null)
            {
                await FollowupAsync($"找不到 ID `{lottery_id}` 的抽獎活動", ephemeral: true);
                return;
            }
            Logger.LogDebug("選擇的抽獎活動：" + Lottery?.ToJson(true));

            var embed = await Lottery.GetEmbedBuilder(Context.Client);
            var componentBuilder = Lottery?.GetComponentBuilder();
            var timestamp = DateTime.Now.AddSeconds(20).ToDiscordTimestamp();
            
            await FollowupAsync($"此面板將在{timestamp}失效", [embed.Build()], components: componentBuilder?.Build(), ephemeral: true);

            await Task.Delay(20000);
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = "";
                msg.Components = new ComponentBuilder().Build();
            });
        }

        [ComponentInteraction("lottery_edit:*")]
        public async Task EditLotteryAsync(string lotteryId)
        {
            long id = long.Parse(lotteryId);
            var Lottery = DbContext.Get<LOTTERY_EVENT_MASTER>(id);
            if(Lottery == null)
            {
                await RespondAsync("找不到抽獎活動");
                return;
            }

            var modal = new ModalBuilder("編輯抽獎", $"lottery_edit_modal:{id}")
                .AddTextInput("抽獎名稱（必填）", "lottery_name", placeholder: "例如：歐了一把，抽2張月卡", value: Lottery.LEM_NAME, required: true)
                .AddTextInput("獎品數（必填）", "lottery_count", placeholder: "需為正整數，例：2", value: $"{Lottery.LEM_PRIZE_AMOUNT}", required: true)
                .AddTextInput("預計抽獎時間（選填）", "lottery_time", placeholder: "例：2025-08-31 20:25:39", value: $"{Lottery.LEM_DRAW_TIME:yyyy-MM-dd HH:mm:ss}", required: false)
                .AddTextInput("抽獎說明（選填）", "lottery_desc", TextInputStyle.Paragraph, placeholder: "其他關於本活動的備註，像是特殊規則或說明", value: $"{Lottery.LEM_DESC}", required: false);

            await RespondWithModalAsync(modal.Build());
        }

        [ComponentInteraction("lottery_toggle:*")]
        public async Task ToggleLotteryAsync(string lotteryId)
        {
            long id = long.Parse(lotteryId);
            var Lottery = DbContext.Get<LOTTERY_EVENT_MASTER>(id);
            if (Lottery == null)
            {
                await RespondAsync("找不到抽獎活動");
                return;
            }
            if (Lottery.LEM_STATUS == "CLOSE" || Lottery.LEM_STATUS == "END" || Lottery.LEM_STATUS == "DRAW")
            {
                await RespondAsync($"抽獎`{Lottery.LEM_NAME}`已經關閉", ephemeral: true);
                return;
            }
            await DeferAsync();
            string responseText = "";
            var (Success, Message) = await DbContext.RunTxAsync(async () =>
            {
                var dict = new Dictionary<string, object?>();
                var time = DateTime.Now;
                if (Lottery.LEM_STATUS == "NEW")
                {
                    dict.Add("LEM_STATUS", "OPEN");
                    dict.Add("LEM_START_TIME", $"{time:yyyy-MM-dd HH:mm:ss}");
                    DbContext.Update<LOTTERY_EVENT_MASTER>(id, dict);
                    var refresh = DbContext.Get<LotteryEvent>(id) ?? throw new Exception("找不到抽獎活動");
                    var owner = await Context.Client.GetUserAsync(refresh.LEM_OWNER_ID);
                    var embed = refresh.GetParticipateEmbed(Context.Guild, owner);
                    var component = refresh.GetParticipateComponent();
                    var msg = await Context.Channel.SendMessageAsync($"抽獎`{Lottery.LEM_NAME}`開始", embed: embed.Build(), components:component.Build());
                    await msg.PinAsync();
                    dict.Clear();
                    dict.Add("LEM_GUILD_ID", Context.Guild.Id);
                    dict.Add("LEM_CHANNEL_ID", msg.Channel.Id);
                    dict.Add("LEM_MESSAGE_ID", msg.Id);
                    DbContext.Update<LOTTERY_EVENT_MASTER>(id, dict);
                    responseText = $"抽獎活動 `{Lottery.LEM_NAME}` 已於{time.ToDiscordTimestamp()}開放參加。";
                }
                else if(Lottery.LEM_STATUS == "OPEN")
                {
                    var msg = await Context.Client.GetMessageAsync(Lottery.LEM_CHANNEL_ID.Value, Lottery.LEM_MESSAGE_ID.Value) ?? throw new Exception($"找不到抽獎參與訊息。");
                    await msg.UnpinAsync();
                    await msg.Channel.SendMessageAsync($"抽獎活動 `{Lottery.LEM_NAME}` 已截止\n連結：{msg.GetJumpUrl()}");

                    dict.Add("LEM_STATUS", "DRAW");
                    dict.Add("LEM_END_TIME", $"{time:yyyy-MM-dd HH:mm:ss}");
                    DbContext.Update<LOTTERY_EVENT_MASTER>(id, dict);
                    responseText = $"抽獎活動 `{Lottery.LEM_NAME}` 已於{time.ToDiscordTimestamp()}截止參加。";
                }
                else
                {
                    throw new Exception($"狀態異常 - {Lottery.LEM_STATUS}");
                }
            });
            if (!Success)
            {
                await FollowupAsync($"操作失敗：{Message}", ephemeral: true);
            }

            var Lottery_Now = GetLotteryEvent(id);
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Components = Lottery_Now.GetComponentBuilder().Build();
            });
            // TODO: 切換活動狀態（開放/截止）
            await FollowupAsync(responseText, ephemeral: true);
        }

        [ComponentInteraction("lottery_close:*")]
        public async Task CloseLotteryAsync(string lotteryId)
        {
            long id = long.Parse(lotteryId);
            var Lottery = DbContext.Get<LOTTERY_EVENT_MASTER>(id);
            if (Lottery == null)
            {
                await RespondAsync("找不到抽獎活動");
                return;
            }
            var dict = new Dictionary<string, object?>
            {
                {"LEM_CLOSE_TIME", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"},
                {"LEM_STATUS", "CLOSE"},
            };
            var (Success, Message) = DbContext.RunTx(() =>
            {
                DbContext.Update<LOTTERY_EVENT_MASTER>(id, dict);
            });
            if (!Success)
            {
                Logger.LogWarning($"抽獎`{Lottery.LEM_NAME}`關閉失敗:{Lottery.ToJson()}");
                await RespondAsync($"抽獎`{Lottery.LEM_NAME}`關閉失敗，請稍後再試", ephemeral: true);
            }
            // TODO: 關閉活動
            await RespondAsync($"抽獎`{Lottery.LEM_NAME}`已關閉。", ephemeral: true);
        }


        private LotteryEvent GetLotteryEvent(long id)
        {
            var Lottery = DbContext.Get<LotteryEvent>(id) ?? throw new ArgumentNullException($"查無編號為{id}的抽獎活動");
            var param = new DynamicParameters();
            param.Add("EventId", id);
            var sql_p = "SELECT * FROM LOTTERY_EVENT_PARTICIPANT WHERE LEP_LEM_SEQ = @EventId";
            Lottery.Participants = DbContext.Query<LOTTERY_EVENT_PARTICIPANT>(sql_p, param).ToList();
            var sql_w = "SELECT * FROM LOTTERY_EVENT_WINNER WHERE LEW_LEM_SEQ = @EventId";
            Lottery.Winners = DbContext.Query<LOTTERY_EVENT_WINNER>(sql_w, param).ToList();
            return Lottery;
        }

        public class LotteryIdAutocomplete(DatabaseContext db, ILogger<LotteryIdAutocomplete> logger) : AutocompleteHandler
        {
            private readonly DatabaseContext _DbContext = db;

            public override Task<AutocompletionResult> GenerateSuggestionsAsync(
                IInteractionContext context,
                IAutocompleteInteraction autocompleteInteraction,
                IParameterInfo parameter,
                IServiceProvider services)
            {
                string input = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

                var sql = "SELECT * FROM LOTTERY_EVENT_MASTER WHERE LEM_OWNER_ID = @UserId";
                var param = new DynamicParameters();
                param.Add("UserId", context.User.Id);
                if (!string.IsNullOrWhiteSpace(input))
                {
                    sql += " AND LEM_NAME LIKE @EventName";
                    param.Add("EventName", $"%{input}%");
                }
                var Events = _DbContext.Query<LOTTERY_EVENT_MASTER>(sql, param).ToList();
                var suggestions = Events.ConvertAll(e => new AutocompleteResult(e.LEM_NAME, e.Id));
                foreach(var e in Events)
                {
                    logger.LogDebug($"{e.Id} - {e.LEM_NAME}");
                }
                return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
            }
        }
    }
}
