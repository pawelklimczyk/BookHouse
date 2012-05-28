using System.Windows.Input;

namespace BooksHouse
{
    public static class CommandLibrary
    {
        private static readonly RoutedUICommand addBookItem = new RoutedUICommand("Add Book", "AddBook", typeof(CommandLibrary));
        private static readonly RoutedUICommand addBookToCategoryItem = new RoutedUICommand("Add Book To Category", "AddBookToCategory", typeof(CommandLibrary));
        private static readonly RoutedUICommand editCategoryItem = new RoutedUICommand("Edit Category", "EditCategory", typeof(CommandLibrary));
        private static readonly RoutedUICommand refreshCategoryList = new RoutedUICommand("Refresh Category List", "RefreshCategoryList", typeof(CommandLibrary));
        private static readonly RoutedUICommand addCategory = new RoutedUICommand("Add Category", "AddCategory", typeof(CommandLibrary));
        private static readonly RoutedUICommand removeCategory = new RoutedUICommand("Remove Category", "RemoveCategory", typeof(CommandLibrary));
        private static readonly RoutedUICommand searchBook = new RoutedUICommand("Search Book", "SearchBook", typeof(CommandLibrary));
        private static readonly RoutedUICommand changeSkin = new RoutedUICommand("Change Skin", "ChangeSkin", typeof(CommandLibrary));

        public static RoutedUICommand AddCategoryItem
        {
            get { return addCategory; }
        }

        public static RoutedUICommand RemoveCategoryItem
        {
            get { return removeCategory; }
        }

        public static RoutedUICommand RefreshCategoryList
        {
            get { return refreshCategoryList; }
        }

        public static RoutedUICommand EditCategoryItem
        {
            get { return editCategoryItem; }
        }

        public static RoutedUICommand AddBookToCategoryItem
        {
            get { return addBookToCategoryItem; }
        }

        public static RoutedUICommand AddBookItem
        {
            get { return addBookItem; }
        }

        public static RoutedUICommand SearchBookItem
        {
            get { return searchBook; }
        }

        public static RoutedUICommand ChangeSkinItem
        {
            get { return changeSkin; }
        }
    }
}