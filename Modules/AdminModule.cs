using Discord.Commands;
using Discord.Interactions;

namespace ArchonBot.Modules
{
    public class AdminModule(CommandService commands, InteractionService interactions, IServiceProvider provider,
                             ILogger<AdminModule> logger, AdminService adminService, DatabaseContext dbContext, BotService bot) 
                             : BaseInteractionModule(interactions, bot, dbContext, adminService, logger)
    {
        private readonly CommandService _commands = commands;
        private readonly IServiceProvider _provider = provider;

        [SlashCommand("timestamp", "取得指定時間的Discord時間戳")]
        public async Task TimestampAsync(int year, int month, int day, int hour, int minute, int second)
        {
            await DeferAsync(ephemeral: true);
            var time = new DateTime(year, month, day, hour, minute, second);
            var embed = new EmbedBuilder().WithTitle("Discord時間戳").WithDescription($"指定時間：{time}");
            var all_formats = "" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.ShortTime)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.ShortTime)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.LongTime)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.LongTime)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.ShortDate)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.ShortDate)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.LongDate)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.LongDate)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.ShortDateTime)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.ShortDateTime)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.LongDateTime)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.LongDateTime)}`\n" +
                $"· {time.ToDiscordTimestamp(DiscordTimestampFormat.Relative)}\n> `{time.ToDiscordTimestamp(DiscordTimestampFormat.Relative)}`\n";
            embed.AddField("各時間戳格式", all_formats);

            await FollowupAsync("", embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("module-load", "動態載入指定模組")]
        public async Task LoadModule(string moduleName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == moduleName);

            if (type == null)
            {
                await RespondAsync($"❌ 找不到模組 `{moduleName}`");
                return;
            }

            await _commands.AddModuleAsync(type, _provider);
            await _interactions.AddModuleAsync(type, _provider);

            await RespondAsync($"✔️ 模組 `{moduleName}` 已載入");
        }

        [SlashCommand("module-unload", "卸載指定模組")]
        public async Task UnloadModule(string moduleName)
        {
            var cmdModule = _commands.Modules.FirstOrDefault(x => x.Name == moduleName);
            var intModule = _interactions.Modules.FirstOrDefault(x => x.Name == moduleName);

            if (cmdModule == null && intModule == null)
            {
                await RespondAsync($"❌ 找不到模組 `{moduleName}`");
                return;
            }

            if (cmdModule != null)
                await _commands.RemoveModuleAsync(cmdModule);

            if (intModule != null)
                await _interactions.RemoveModuleAsync(intModule);

            await RespondAsync($"🗑 已卸載 `{moduleName}`");
        }

        [SlashCommand("module-reload", "重新載入指定模組")]
        public async Task ReloadModule(string moduleName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == moduleName);

            if (type == null)
            {
                await RespondAsync($"❌ 找不到模組 `{moduleName}`");
                return;
            }

            // 卸載
            var cmdModule = _commands.Modules.FirstOrDefault(x => x.Name == moduleName);
            var intModule = _interactions.Modules.FirstOrDefault(x => x.Name == moduleName);

            if (cmdModule != null)
                await _commands.RemoveModuleAsync(cmdModule);

            if (intModule != null)
                await _interactions.RemoveModuleAsync(intModule);

            // 再載入
            await _commands.AddModuleAsync(type, _provider);
            await _interactions.AddModuleAsync(type, _provider);

            await RespondAsync($"🔄 已重新載入 `{moduleName}`");
        }


        [SlashCommand("add-param-m", "新增全域參數類別")]
        public async Task AddParamType(
            [Discord.Interactions.Summary("模組別", "參數類型所屬模組")] string moduleId,
            [Discord.Interactions.Summary("類型代號", "參數類型代號，不可重複")] string paramType,
            [Discord.Interactions.Summary("類型說明", "(選填)描述此參數類別的用途")] string? paramTypeDesc = null)
        {
            await DeferAsync();
            var ParamD = new BOT_PARAM_MASTER
            {
                BPM_BOT_ID = AdminService.GetBotId(),
                BPM_MODULE_ID = moduleId,
                BPM_PARAM_TYPE = paramType,
                BPM_PARAM_DESC = paramTypeDesc
            };
            var lst = new List<string>
            {
                "BPM_BOT_ID",
                "BPM_MODULE_ID",
                "BPM_PARAM_TYPE",
                "BPM_PARAM_DESC"
            };
            var dict = ParamD.ToDictionary(lst);
            var result = DbContext.RunTx(() =>
            {
                DbContext.Insert<BOT_PARAM_MASTER>(dict);
            });
            if (!result.Success)
            {
                await FollowupAsync($"新增失敗：{result.Message}");
                return;
            }
            await FollowupAsync($"新增成功！");
        }

        [SlashCommand("add-param-d", "新增全域參數")]
        public async Task AddParamType(
            [Autocomplete(typeof(ModuleIdAutocomplete))]
            [Discord.Interactions.Summary("模組別", "參數所屬模組")] string moduleId,
            [Discord.Interactions.Summary("類型", "參數所屬類型")] string paramType,
            [Discord.Interactions.Summary("代號", "參數代號，不可重複")] string paramId,
            [Discord.Interactions.Summary("參數值", "參數對應的數值")] string paramValue,
            [Discord.Interactions.Summary("名稱", "參數名稱，僅方便辨識用")] string? paramName = null,
            [Discord.Interactions.Summary("屬性", "參數屬性，作為額外資料記錄欄位")] string? paramAttr = null,
            [Discord.Interactions.Summary("說明", "參數說明，紀錄參數用途")] string? paramDesc = null)
        {
            await Context.Interaction.DeferAsync();
            Logger.LogInformation($"{moduleId}, {paramType}, {paramId}, {paramValue}, {paramName}, {paramAttr}, {paramDesc}");
        }
    }

    public class ModuleIdAutocomplete : AutocompleteHandler
    {
        private readonly DatabaseContext _DbContext;
        public ModuleIdAutocomplete(DatabaseContext db) {
            _DbContext = db;
        }
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            string input = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

            var sql = "SELECT DISTINCT BPM_MODULE_ID FROM BOT_PARAM_MASTER WHERE UPPER(BPM_MODULE_ID) LIKE @Input";
            var param = new DynamicParameters();
            param.Add("Input", $"%{input.ToUpper()}%");
            var ModuleIds = _DbContext.Query<string>(sql, param).ToList();
            var suggestions = ModuleIds.ConvertAll(f => new AutocompleteResult(f, f));
            return Task.FromResult(AutocompletionResult.FromSuccess(suggestions));
        }
    }
}
