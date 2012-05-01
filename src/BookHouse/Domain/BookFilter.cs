namespace BooksHouse.Domain
{
    public class BookFilter
    {
        public BookFilter() { }
        public BookFilter(BookFilter filter)
        {
            this.Title = filter.Title;
            this.Author = filter.Author;
            this.AdditionalInfo = filter.AdditionalInfo;
            this.ISBN = filter.ISBN;
            this.RootCategoryId = filter.RootCategoryId;
        }

        public bool HasTextFilter
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Author) ||
                       !string.IsNullOrWhiteSpace(AdditionalInfo) || !string.IsNullOrWhiteSpace(ISBN);
            }
        }

        public string Title { get; set; }
        public string Author { get; set; }
        public string AdditionalInfo { get; set; }
        public string ISBN { get; set; }
        public long RootCategoryId { get; set; }
    }
}
