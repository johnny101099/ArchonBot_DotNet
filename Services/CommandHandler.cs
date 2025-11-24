using Discord.Commands;
using Microsoft.Extensions.Options;

namespace ArchonBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly BotSetting _config;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IOptions<BotSetting> config, ILogger<CommandHandler> logger)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _config = config.Value;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _logger.LogInformation("===== Loaded Text Command Modules =====");

            foreach (var module in _commands.Modules)
            {
                _logger.LogInformation($"[CommandModule] {module.Name}");

                string info = $"● [CommandModule] {module.Name}";

                foreach (var cmd in module.Commands)
                    info += $"\n└─ Command: {cmd.Name} | Alias: {string.Join(", ", cmd.Aliases)}";
                _logger.LogInformation(info);
            }
        }

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            // 忽略系統或機器人訊息
            if (rawMessage is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!message.HasStringPrefix(_config.Prefix, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync($"⚠️ 錯誤: {result.ErrorReason}");
            }
        }
    }
}
