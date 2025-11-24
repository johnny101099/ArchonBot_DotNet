namespace ArchonBot.Extensions
{
    public static class ObjectExtensions
    {
        public static Hashtable ToHashtable(this object obj)
        {
            var ht = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (var item in obj.GetType().GetProperties())
            {
                ht[item.Name] = item.GetValue(obj, null);
            }
            return ht;
        }
        public static Hashtable ToHashtable(this object obj, string[] includeProperties)
        {
            var ht = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in obj.GetType().GetProperties())
            {
                var name = prop.GetColName();
                if (includeProperties.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    ht[name] = prop.GetValue(obj, null);
                }
            }
            return ht;
        }
        public static ExpandoObject Merge<TLeft, TRight>(this TLeft left, TRight right)
        {
            var expando = new ExpandoObject();
            IDictionary<string, object> dict = expando;
            foreach (var p in left.GetType().GetProperties())
                dict[p.Name] = p.GetValue(left);
            foreach (var p in right.GetType().GetProperties())
                dict[p.Name] = p.GetValue(right);
            return expando;
        }

        /// <summary>將物件序列化成JSON字串</summary>
        /// <param name="obj"></param>
        /// <param name="indent">是否啟用縮排(用於某些需要印出json的場景)，預設為<see langword="false"/>。</param>
        /// <returns>物件轉換成的JSON字串</returns>
        public static string ToJson(this object obj, bool indent = false)
        {
            var options = new JsonSerializerOptions {
                // 讓非 ASCII 字元不被轉成 \uXXXX
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
                // 是否縮排
                WriteIndented = indent,
            };
            return JsonSerializer.Serialize(obj, options);
        }
    }
}

