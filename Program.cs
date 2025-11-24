using ArchonBot.Util;
//using AutoMapper;
using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.Logging.Console;
using System.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        //  Log 服務
        services.AddLogging(logging =>
        {
            logging.ClearProviders(); // 清掉預設 Console 格式
            logging.SetMinimumLevel(LogLevel.Trace);

            logging.AddConsole(options =>
            {
                options.FormatterName = "custom"; // 使用我們自訂的 Formatter
            });

            logging.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
        });
        //  取得Config
        services.Configure<BotSetting>(context.Configuration.GetSection("Bot"));
        //  設定Discord應用程式意圖
        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        //  建立Discord客戶端實例
        var client = new DiscordSocketClient(socketConfig);
        services.AddSingleton(client);
        //  訊息指令服務
        services.AddSingleton<CommandService>();
        //  應用指令服務
        services.AddSingleton(x =>
        {
            var discordClient = x.GetRequiredService<DiscordSocketClient>();
            return new InteractionService(discordClient.Rest);
        });
        //  訊息指令處理程式
        services.AddSingleton<CommandHandler>();
        //  應用指令處理程式
        services.AddSingleton<InteractionHandler>();
        //  資料庫連線
        services.AddTransient<IDbConnection>(sp => new SqliteConnection(context.Configuration["Database:ConnectionString"]!));
        //  資料庫工具
        services.AddSingleton<DatabaseContext>();
        //  機器人服務
        services.AddSingleton<BotService>();
        //  Admin服務(包含各種)
        services.AddSingleton<AdminService>();
    })
    .Build();

var bot = host.Services.GetRequiredService<BotService>();
await bot.StartAsync();