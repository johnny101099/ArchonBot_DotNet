namespace ArchonBot.Extensions
{
    public static class StringExtensions
    {
        /// <summary>將字串反序列化成 <typeparamref name="T"/> 物件。</summary>
        /// <typeparam name="T">反序列化的目標型別</typeparam>
        /// <param name="input">要進行反序列化的json字串</param>
        public static T? DeserializeTo<T>(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? default : JsonSerializer.Deserialize<T>(input);
        }
    }
}