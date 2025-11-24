namespace ArchonBot.Extensions
{
    public static class TypeExtensions
    {
        public static string GetTableName(this Type type)
        {
            var name = type.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault()?.Name ?? type.Name;
            return name;
        }
        public static string GetColName(this PropertyInfo prop)
        {
            var name = prop.GetCustomAttributes(false).OfType<ColumnAttribute>().FirstOrDefault()?.Name ?? prop.Name;
            return name;
        }
        public static string GetKeyName(this Type type)
        {
            var keys = type.GetProperties().Where(p => p.GetCustomAttributes(false).Any(a => a is KeyAttribute)).ToList();
            if (keys.Count == 0)
            {
                throw new NullReferenceException($"Type {type.FullName} 沒有包含 [Key] 屬性的欄位");
            }
            var name = keys.First().GetCustomAttributes(false).OfType<ColumnAttribute>().FirstOrDefault()?.Name ?? keys.First().Name;
            return name;
        }
    }
}
