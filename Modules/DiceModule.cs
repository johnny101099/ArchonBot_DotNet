using ArchonBot.Extensions;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace ArchonBot.Modules
{
    public class DiceModule : BaseCommandModule
    {
        public static readonly Regex BASIC_DICE_PATTERN = new(
            @"^\s*([1-9]\d*)?\s*[dD]\s*(-?[1-9]\d*|0)\s*",
            RegexOptions.Compiled
        );

        public static readonly Regex RANGE_DICE_PATTERN = new Regex(
            @"^\s*([1-9]\d*)?\s*[dD]\s*[\(\[\{]\s*(-?[1-9]\d*|0)\s*[~]\s*(-?[1-9]\d*|0)\s*[\)\]\}]\s*",
            RegexOptions.Compiled
        );

        public static readonly Regex OPTION_DICE_PATTERN = new Regex(
            @"^\s*([1-9]\d*)?\s*[dD]\s*[\(\[\{]\s*[^\,\)\]\}\{\[\(]+\s*([\u3001,\uff0c]\s*[^\,\)\]\}\{\[\(]+\s*)*[\)\]\}]\s*",
            RegexOptions.Compiled
        );
        private static readonly char[] DiceKey = ['d', 'D'];

        public DiceModule(DatabaseContext db, DiscordSocketClient client, ILogger<BaseCommandModule> logger, CommandService commands) : base(db, client, logger, commands)
        {

        }

        internal enum DiceMode
        {
            /// <summary>基礎</summary>
            Basic,
            /// <summary>範圍</summary>
            Range,
            /// <summary>選項</summary>
            Option
        }

        internal class DiceResult(DiceCommand cmd, List<List<string>> result)
        {
            /// <summary>擲骰指令</summary>
            public DiceCommand Command { get; set; } = cmd;
            /// <summary>擲骰原始結果</summary>
            public List<List<string>> Result { get; set; } = result;

            /// <summary>擲骰統計結果</summary>
            public Dictionary<string, int> Statistics => Result.SelectMany(x => x)
                                                               .GroupBy(x => x)
                                                               .ToDictionary(g => g.Key, g => g.Count());

            /// <summary>擲骰最多結果</summary>
            public Dictionary<string, int> Most {
                get {
                    var max = Statistics.Values.Max();
                    return Statistics.Where(kv => kv.Value == max).ToDictionary();
                }
            }

            public Embed GetResultEmbed()
            {
                var embed = new EmbedBuilder{
                    Title = "",
                    Description = "",
                };
                string ResultStr = "";
                if (Command.Round == 1)
                {
                    ResultStr = string.Join(", ", Result[0]);
                }
                else
                {
                    var rounds = Result.Select((r, i) => $"`({i + 1})` {string.Join(", ", r)}");
                    ResultStr = $"擲骰結果:\n{string.Join("\n", rounds)}";
                }
                if(ResultStr.Length >= 900)
                {
                    ResultStr = ResultStr[..ResultStr.LastIndexOf("\n")] + "\n...";
                }
                if(Result.SelectMany(x => x).Count() >= 10)
                {
                    var statisticsStr = "";

                }

                return embed.Build();
            }
        }

        internal class DiceCommand
        {
            /// <summary>擲骰次數</summary>
            public int Number { get; set; } = 1;
            /// <summary>擲骰模式</summary>
            public DiceMode Mode { get; set; } = DiceMode.Basic;
            /// <summary>Basic/Range骰子最小值</summary>
            public int Min { get; set; } = 0;
            /// <summary>Basic/Range最大值</summary>
            public int Max { get; set; } = 0;
            /// <summary>Option項目</summary>
            public List<string> Options { get; set; } = [];
            /// <summary>擲骰輪次</summary>
            public int Round { get; set; } = 1;
            /// <summary>指令是否合法</summary>
            public bool IsValid { get; set; } = false;
            /// <summary>錯誤訊息</summary>
            public string ErrorMessage { get; set; } = string.Empty;
            /// <summary>原始擲骰參數</summary>
            public string OriginalParam { get; set; } = string.Empty;
            /// <summary>擲骰說明/備註 清單</summary>
            public List<string> Remark { get; set; } = [];
            /// <summary>擲骰說明/備註 內容</summary>
            public string RemarkContent => string.Join(" ", Remark);

            /// <summary>執行擲骰</summary>
            /// <returns>擲骰結果</returns>
            public DiceResult GetDiceResult()
            {
                var all_result = new List<List<string>>();
                var optionMode = Mode == DiceMode.Option;
                var minValue = optionMode ? 0 : Min;
                var maxValue = optionMode ? Options.Count : Max + 1;
                for (int r = 0; r < Round; r++)
                {
                    var round_result = new List<string>();
                    for (int d = 0; d < Number; d++)
                    {
                        var SingleResult = Random.Shared.Next(minValue, maxValue);
                        round_result.Add(optionMode ? Options[SingleResult] : optionMode.ToString());
                    }
                    all_result.Add(round_result);
                }
                return new DiceResult(this, all_result);
            }
        }

        [Command("dice")]
        [Alias(["DICE", "d", "D"])]
        [Summary("進行擲骰")]
        public async Task DiceAsync(string DiceParam, params string[] ExtraParam)
        {
            Logger.LogInformation($"Dice command invoked with parameters: {DiceParam}, {string.Join(", ", ExtraParam)}");
            var cmd = ParseDiceParam(DiceParam.Trim(), ExtraParam);
            Logger.LogInformation($"Parsed DiceCommand:{cmd.ToJson(true)}");
            var result = cmd.GetDiceResult();
            Logger.LogInformation($"Parsed DiceCommand:{result.ToJson(true)}");
            await Context.Message.ReplyAsync("WIP");
        }

        private static DiceCommand ParseDiceParam(string DiceParam, string[] ExtraParam)
        {
            var cmd = new DiceCommand {
                OriginalParam = DiceParam
            };
            var d_index = DiceParam.IndexOfAny(DiceKey);
            if(d_index == -1)
            {
                cmd.IsValid = false;
                cmd.ErrorMessage = "擲骰參數錯誤: 缺少 'd' 或 'D'，無法解析指令。";
                return cmd;
            }
            //var HasNumber = int.TryParse(DiceParam[..d_index].Trim(), out var number);
            cmd.Number = int.TryParse(DiceParam[..d_index].Trim(), out var number) ? number : 1;
            var paramStr = DiceParam[(d_index + 1)..].Trim();
            if (BASIC_DICE_PATTERN.IsMatch(DiceParam))
            {
                cmd.Mode = DiceMode.Basic;
                var success = int.TryParse(paramStr, out var max);
                if(!success)
                {
                    cmd.IsValid = false;
                    cmd.ErrorMessage = $"擲骰參數錯誤: 擲骰面數 '{paramStr}' 無法解析為整數。";
                    return cmd;
                }
                cmd.Min = 1;
                cmd.Max = max;
            }
            if (RANGE_DICE_PATTERN.IsMatch(DiceParam))
            {
                cmd.Mode = DiceMode.Range;
                var ranges = paramStr[1..^1].Trim().Split("~").Select(s => new { success = int.TryParse(s.Trim(), out var num), value = num });
                if (ranges.Count() != 2 || !ranges.All(r => r.success))
                {
                    cmd.IsValid = false;
                    cmd.ErrorMessage = $"擲骰參數錯誤: 範圍參數 '{paramStr}' 無法解析為有效範圍。";
                    return cmd;
                }
                cmd.Min = ranges.Min(a => a.value);
                cmd.Max = ranges.Max(a => a.value);
            }
            if (OPTION_DICE_PATTERN.IsMatch(DiceParam))
            {
                cmd.Mode = DiceMode.Option;
                var options = paramStr[1..^1].Trim().Split([',', '、', '，']).Select(s => s.Trim()).Where(o => o.Length > 0).ToList();
                if (options.Count == 0)
                {
                    cmd.IsValid = false;
                    cmd.ErrorMessage = $"擲骰參數錯誤: 選項參數 '{paramStr}' 無有效選項。";
                    return cmd;
                }
                cmd.Options = options;
            }
            var HasRound = int.TryParse(ExtraParam[0], out var round);
            cmd.Round = HasRound ? round : 1;
            cmd.Remark = ExtraParam.Skip(HasRound ? 1 : 0).ToList();
            cmd.IsValid = true;
            return cmd;
        }
    }
}
