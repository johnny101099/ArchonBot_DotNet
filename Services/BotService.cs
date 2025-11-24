using Microsoft.VisualBasic;

namespace ArchonBot.Services
{
    public class BotService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _cmdHandler;
        private readonly InteractionHandler _intHandler;
        private readonly BotSetting _config;
        private readonly DatabaseContext _dbContext;
        private readonly ILogger<BotService> _logger;

        public BotService(
            DiscordSocketClient client,
            CommandHandler cmdHandler,
            InteractionHandler intHandler,
            IOptions<BotSetting> config,
            DatabaseContext dbContext,
            ILogger<BotService> logger)
        {
            _client = client;
            _cmdHandler = cmdHandler;
            _intHandler = intHandler;
            _config = config.Value;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            _client.Log += LogAsync;
            _client.Ready += OnBotReady;

            // 初始化文字與 Slash 指令
            await _cmdHandler.InitializeAsync();
            await _intHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnBotReady()
        {
            _logger.LogInformation($"[{_config.Id}] {_client.CurrentUser.Username} Ready!");

            await _intHandler.RegisterCommandsAsync();
            _logger.LogInformation("All subsystems ready.");
        }

        private Task LogAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(msg.Exception, msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(msg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Message);
                    break;
                default:
                    _logger.LogInformation(msg.Message);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
