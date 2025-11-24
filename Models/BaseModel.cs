namespace ArchonBot.Models
{
    /// <summary>資料表基礎class</summary>
    public abstract class BaseModel
    {
        /// <summary>資料索引，繼承此基底class的各資料表class應定義此欄位值等於各Table的索引欄位值</summary>
        public abstract long Id { get; }

        public bool NewCreate => Id == 0;

        public object? Temp1 { get; set; }
        public object? Temp2 { get; set; }
        public object? Temp3 { get; set; }
        public object? Temp4 { get; set; }
        public object? Temp5 { get; set; }
        public object? Temp6 { get; set; }
        public object? Temp7 { get; set; }
        public object? Temp8 { get; set; }
        public object? Temp9 { get; set; }
        public object? Temp10 { get; set; }

        public Dictionary<string, object?> ToDictionary()
        {
            var dict = new Dictionary<string, object?>();
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                dict[prop.Name] = prop.GetValue(this);
            }
            return dict;
        }

        public Dictionary<string, object?> ToDictionary(IEnumerable<string> includeProperties)
        {
            var dict = new Dictionary<string, object?>();
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {   
                if (includeProperties.Contains(prop.Name))
                {
                    dict[prop.Name] = prop.GetValue(this);
                }
            }
            return dict;
        }
    }
}
