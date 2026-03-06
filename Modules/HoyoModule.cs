using Discord.Interactions;
using EnkaDotNet;
using EnkaDotNet.Enums.Genshin;

namespace ArchonBot.Modules
{
    public class HoyoModule : BaseInteractionModule
    {
        private IEnkaClient _enkaClient;
        public HoyoModule(
            InteractionService interactions,
            BotService bot,
            DatabaseContext dbContext,
            AdminService adminService,
            ILogger<LotteryModule> logger,
            IEnkaClient enkaClient)
            : base(interactions, bot, dbContext, adminService, logger)
        {
            _enkaClient = enkaClient;
        }

        private async Task InitializeEnkaClientAsync()
        {
            if (_enkaClient == null)
            {
                var options = new EnkaClientOptions
                {
                    EnableCaching = true,
                    PreloadLanguages = ["zh-tw"],
                    UserAgent = "TestBot/1.0",
                    CacheProvider = EnkaDotNet.Caching.CacheProvider.SQLite,
                    SQLiteCache = new EnkaDotNet.Caching.SQLiteCacheOptions
                    {
                        DatabasePath = "EnkaCache.db",
                    },
                };
                _enkaClient = await EnkaClient.CreateAsync(options);
            }
        }

        // genshin_profile
        [SlashCommand("genshin_profile", "原神角色資訊")]
        public async Task GetGenshinProfileAsync(int uid)
        {
            await DeferAsync();
            await InitializeEnkaClientAsync();
            var (playerInfo, characters) = await _enkaClient.GetGenshinUserProfileAsync(uid, language: "zh-tw");
            var characterList = string.Join("\n", characters.Select(c => $"**{c.Name}** - 等級 {c.Level} - 命座 {c.ConstellationLevel}"));
            await FollowupAsync(characterList);
        }

        // genshin_profile_admin
        [SlashCommand("genshin_profile_admin", "原神角色資訊(我的)")]
        public async Task GetMyGenshinProfileAsync()
        {
            await DeferAsync();
            await InitializeEnkaClientAsync();
            var (playerInfo, characters) = await _enkaClient.GetGenshinUserProfileAsync(811437931, language: "zh-tw", bypassCache: true);
            var characterList = string.Join("\n", characters.Select(c => $"**{c.Name}** - 等級 {c.Level} - 命座 {c.ConstellationLevel}"));
            await FollowupAsync(components: GetCharaterComponent(characters).Build());
        }

        private ComponentBuilderV2 GetCharaterComponent(IReadOnlyList<EnkaDotNet.Components.Genshin.Character> Charaters)
        {
            var components = new ComponentBuilderV2([
                new ContainerBuilder()
                    .WithAccentColor(9225410)
                    .WithTextDisplay(
                        new TextDisplayBuilder().WithContent("請選擇要檢視的角色")
                    )
                    .WithActionRow(
                        new ActionRowBuilder()
                            .WithComponents([
                                new SelectMenuBuilder()
                                    .WithType(ComponentType.SelectMenu)
                                    .WithCustomId("99fca274f90a4070beb7086fdf335bfc")
                                    .WithPlaceholder("請選擇要檢視的角色...")
                                    .WithOptions(
                                        [.. Charaters.Select(c => new SelectMenuOptionBuilder()
                                            .WithLabel($"{c.Name} ({c.Level}等)")
                                            .WithValue(c.Id.ToString())
                                            .WithDescription($"{c.ConstellationLevel}命{c.Weapon.Refinement}精 | 遊戲內展示櫃")
                                            .WithEmote(
                                                c.Element == ElementType.Anemo ? new Emote(1476156044286758942, "Anemo") :
                                                c.Element == ElementType.Geo ? new Emote(1476156005694963742, "Geo") :
                                                c.Element == ElementType.Electro ? new Emote(1476155910614290443, "Electro") :
                                                c.Element == ElementType.Dendro ? new Emote(1476155963026309204, "Dendro") :
                                                c.Element == ElementType.Hydro ? new Emote(1476155734734672044, "Hydro") :
                                                c.Element == ElementType.Pyro ? new Emote(1476155793366843534, "Pyro") :
                                                c.Element == ElementType.Cryo ? new Emote(1476155849591226520, "Cryo") : new Emoji("❓")
                                            )
                                        )]),
                            ])
                    )
                    .WithActionRow(
                        new ActionRowBuilder()
                            .WithComponents([
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Primary)
                                    .WithLabel("確認")
                                    .WithCustomId("36acfad8d418474084daed1d0d06bef2"),
                                new ButtonBuilder()
                                    .WithStyle(ButtonStyle.Secondary)
                                    .WithLabel("隱藏面板")
                                    .WithCustomId("6e80a02957f040d886c9e7a90d1644b2"),
                            ])
                    )
            ]);
            return components;
        }

        private void GenerateGenshinProfileEmbed()
        {

        }
    }
}
