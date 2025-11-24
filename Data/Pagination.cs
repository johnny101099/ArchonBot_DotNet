namespace ArchonBot.Data
{
    public class Pagination<T>
    {
        private Func<T, string> _formatter = a => $"{a}";
        public IEnumerable<T> Items { get; set; }
        public long Count { get; set; }
        public long PageSize { get; set; }
        public long PageLength { get; set; }
        public long CurrentPage { get; set; }
        public long TotalPages => (long)Math.Ceiling(decimal.Divide(Count, PageSize));
        public bool EnablePrevious => CurrentPage > 1;
        public bool EnableNext => CurrentPage < TotalPages;
        public IList<long> Pages { get; private set; } = new List<long>();
        public Pagination(IEnumerable<T> items, long count, long currentPage, long pageSize, long pageLength)
        {
            Items = items;
            Count = count;
            CurrentPage = currentPage;
            PageSize = pageSize;
            PageLength = pageLength;
            UpdatePages();
        }

        public void SetFormatter(Func<T, string> formatter)
        {
            _formatter = formatter;
        }

        public Embed GetEmbed()
        {
            var builder = new EmbedBuilder();

            return builder.Build();
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in Items)
                yield return item;
        }
        private void UpdatePages()
        {
            var pages = new List<long>();
            var pageIndex = CurrentPage - 1;
            if (pageIndex > TotalPages - PageLength + Math.Floor(PageLength / 2.0m))
            {
                for (var x = Math.Max(TotalPages - PageLength + 1, 1); x <= TotalPages; x++)
                {
                    pages.Add(x);
                }
            }
            else if (pageIndex < (PageLength + 1) - Math.Floor(PageLength / 2.0m))
            {
                for (var x = 1; x <= Math.Min(PageLength, TotalPages); x++)
                {
                    pages.Add(x);
                }
            }
            else
            {
                var startIndex = pageIndex - (long)Math.Floor(PageLength / 2.0m);
                for (var x = startIndex; x <= startIndex + PageLength - 1; x++)
                {
                    pages.Add(x);
                }
            }
            Pages = pages;
        }
    }
}
