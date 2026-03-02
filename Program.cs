using ArchonBot.Util;
//using AutoMapper;
using Discord.Commands;
using Discord.Interactions;
using EnkaDotNet;
using EnkaDotNet.DIExtensions;
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
        //  Enka.NET 客戶端服務
        //services.AddSingleton<IEnkaClient>(sp =>
        //{
        //    var options = new EnkaClientOptions
        //    {
        //        EnableCaching = false,
        //        PreloadLanguages = ["zh-tw"],
        //        UserAgent = "johnny101099/1.0",
        //    };
        //    // 手動 new 出 Client 實例並傳入需要的參數
        //    // 這樣就不會觸發 DI 容器去自動解析 HttpHelper 的建構函式
        //    return EnkaClient.CreateAsync(options).GetAwaiter().GetResult();
        //});

        //services.AddEnkaNetClient(options =>
        //{
        //    options.EnableCaching = false; // Disable caching
        //                                   // options.CacheDurationMinutes will be ignored if EnableCaching is false
        //    options.PreloadLanguages = ["zh-tw"]; // 預載入繁體中文資料
        //    options.UserAgent = "johnny101099";
        //    options.MaxRetries = 3; // 最大重試次數
        //});
    })
    .Build();

var bot = host.Services.GetRequiredService<BotService>();
await bot.StartAsync();