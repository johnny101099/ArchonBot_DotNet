namespace ArchonBot.Extensions
{
    public enum DiscordTimestampFormat
    {
        ShortTime,      // t
        LongTime,       // T
        ShortDate,      // d
        LongDate,       // D
        ShortDateTime,  // f
        LongDateTime,   // F
        Relative        // R
    }
    public static class DateTimeExtensions
    {
        public static string ToDiscordTimestamp(this DateTime? dt, DiscordTimestampFormat format = DiscordTimestampFormat.Relative)
        {
            return dt == null ? string.Empty : dt.Value.ToDiscordTimestamp(format);
        }
        public static string ToDiscordTimestamp(this DateTime dt, DiscordTimestampFormat format = DiscordTimestampFormat.Relative)
        {
            string code = format switch
            {
                DiscordTimestampFormat.ShortTime => "t",
                DiscordTimestampFormat.LongTime => "T",
                DiscordTimestampFormat.ShortDate => "d",
                DiscordTimestampFormat.LongDate => "D",
                DiscordTimestampFormat.ShortDateTime => "f",
                DiscordTimestampFormat.LongDateTime => "F",
                DiscordTimestampFormat.Relative => "R",
                _ => "R",
            };
            var unixTime = ((DateTimeOffset)dt).ToUnixTimeSeconds();
            return $"<t:{unixTime}:{code}>";
        }
    }
}
