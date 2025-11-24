using Discord.Interactions;

namespace ArchonBot.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly BotSetting _config;
        private readonly ILogger<InteractionHandler> _logger;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services, IOptions<BotSetting> config, ILogger<InteractionHandler> logger)
        {
            _client = client;
            _interactions = interactions;
            _services = services;
            _config = config.Value;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            // 綁定互動事件
            _client.InteractionCreated += HandleInteractionAsync;
            _client.ModalSubmitted += HandleModalAsync;
            _interactions.ModalCommandExecuted += ModalCommandExecutedAsync;
            _interactions.SlashCommandExecuted += SlashCommandExecutedAsync;

            // 載入互動模組
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _logger.LogInformation("===== Loaded Interaction Modules =====");

            foreach (var module in _interactions.Modules)
            {
                string info = $"● [InteractionModule] {module.Name}";

                foreach (var slash in module.SlashCommands)
                    info += $"\n└─ Slash: /{slash.Name}";

                foreach (var user in module.ContextCommands)
                    info += $"\n└─ Context: {user.Name}";

                _logger.LogInformation(info);
            }
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            if (interaction is SocketModal modal)
            {
                _logger.LogDebug($"Modal 提交! Custom ID: '{modal.Data.CustomId}'");
                foreach (var component in modal.Data.Components)
                {
                    _logger.LogDebug($"元件 ID: '{component.CustomId}', 值: '{component.Value}'");
                }
            }
            else if (interaction is SocketMessageComponent component)
            {
                _logger.LogDebug($"點擊元件! 元件 ID: '{component.Data.CustomId}'");
            }

            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Interaction Error: {ex.Message}");
            }
        }

        private async Task HandleModalAsync(SocketModal modal)
        {
            try
            {
                _logger.LogDebug("ON HandleModalAsync!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Interaction Error: {ex.Message}");
            }
        }

        public async Task RegisterCommandsAsync()
        {
            var adminModule = _interactions.Modules.FirstOrDefault(m => m.Name == "AdminModule");
            var otherModules = _interactions.Modules.Where(m => m.Name != "AdminModule").ToList();

            if (adminModule != null)
            {
                foreach (var guild_id in _config.AdminServerIds)
                {
                    await _interactions.AddModulesToGuildAsync(guild_id, true, [.. _interactions.Modules]);
                    //await _interactions.AddModulesToGuildAsync(guild_id, true, [adminModule]);
                    //_logger.LogInformation($"註冊adminModule到{guild_id}");

                    //foreach (var module in otherModules)
                    //{
                    //    await _interactions.AddModulesToGuildAsync(guild_id, true, [module]);
                    //    _logger.LogInformation($"註冊{module.Name}到{guild_id}");
                    //}
                }
            }

            //foreach (var module in otherModules)
            //{
            //    await _interactions.AddModulesGloballyAsync(true, [module]);
            //    _logger.LogInformation($"註冊{module.Name}到全域");
            //}

            _logger.LogInformation("✅ 已註冊所有 Slash 指令");
        }

        private async Task SlashCommandExecutedAsync(SlashCommandInfo command, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                _logger.LogError($"❌ SlashCommand Error: /{command.Name} - {result.ErrorReason}");
                try
                {
                    // 已經 defer?
                    if (ctx.Interaction.HasResponded)
                        await ctx.Interaction.FollowupAsync($"❌ 錯誤：{result.ErrorReason}", ephemeral: true);
                    else
                        await ctx.Interaction.RespondAsync($"❌ 錯誤：{result.ErrorReason}", ephemeral: true);
                }
                catch
                {
                    // 回覆失敗就算了
                }
            }
        }

        private async Task ModalCommandExecutedAsync(ModalCommandInfo command, IInteractionContext ctx, IResult result)
        {
            if (!result.IsSuccess)
            {
                _logger.LogError($"❌ SlashCommand Error: /{command.Name} - {result.ErrorReason}");
                try
                {
                    // 已經 defer?
                    if (ctx.Interaction.HasResponded)
                        await ctx.Interaction.FollowupAsync($"❌ 錯誤：{result.ErrorReason}", ephemeral: true);
                    else
                        await ctx.Interaction.RespondAsync($"❌ 錯誤：{result.ErrorReason}", ephemeral: true);
                }
                catch
                {
                    // 回覆失敗就算了
                }
            }
        }
    }
}
