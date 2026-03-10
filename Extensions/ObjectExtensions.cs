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

        /// <summary>
        ///     將 <paramref name="obj"/> 包含在 <paramref name="includeProperties"/> 中的屬性與值轉換為字典。<br/>
        ///     若未傳入 <paramref name="includeProperties"/>，則會包含所有屬性。
        /// </summary>
        /// <param name="obj">要轉換的物件本身</param>
        /// <param name="includeProperties"><paramref name="obj"/>中要轉換成字典的屬性清單</param>
        /// <exception cref="ArgumentNullException">呼叫此方法的物件為<see langword="null"/>時</exception>
        public static Dictionary<string, object?> ToDictionary(this object obj, IEnumerable<string>? includeProperties = null)
        {
            ArgumentNullException.ThrowIfNull(obj);
            // 若有傳入 includeProperties，則產生對應的 HashSet 加速查詢
            HashSet<string>? includeSet = null;
            if (includeProperties != null)
            {
                includeSet = new HashSet<string>(includeProperties, StringComparer.OrdinalIgnoreCase);
            }
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in obj.GetType().GetProperties())
            {
                string keyName;
                //  若 includeSet 不為 null 
                if (includeSet != null)
                {
                    //  取得欄位名稱
                    var colName = prop.GetColName();
                    //  檢查欄位名是否在 includeSet 中，若不在就跳過(continue是直接進入下一個迴圈的意思)
                    if (!includeSet.Contains(colName))
                    {
                        continue;
                    }
                    keyName = colName;
                }
                //  若 includeSet 為 null 就直接使用屬性名稱(與先前不傳入第二個參數時的邏輯一致)
                else
                {
                    keyName = prop.GetColName();
                }
                //  若沒有提前continue掉就代表這個屬性要加到Dictionary中
                dict[keyName] = prop.GetValue(obj, null);
            }
            return dict;
        }

        public static ExpandoObject Merge<TLeft, TRight>(this TLeft left, TRight right)
        {
            var expando = new ExpandoObject();
            IDictionary<string, object?> dict = expando;
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

