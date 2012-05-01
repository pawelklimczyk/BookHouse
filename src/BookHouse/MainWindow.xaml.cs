#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using BooksHouse.Domain;
using BooksHouse.Gui.Dialog;

#endregion

namespace BooksHouse
{
    public partial class MainWindow : Window
    {
        private long selectedCategoryId = 0;

        public MainWindow()
        {
            Config.DatabaseName = "baza.db3";
            InitializeComponent();
            CheckDatabaseFilePresence();
            Loaded += MainWindow_Loaded;
            Focus();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCategoryList();
        }

        private void RefreshCategoryList()
        {
            uxCategoryList.ItemsSource = BooksManager.BooksManager.GetCategoryList(0);
        }

        private void CheckDatabaseFilePresence()
        {
            if (!BooksManager.BooksManager.DatabaseFileExists(Config.DatabaseName))
            {
                var status = BooksManager.BooksManager.CraeteDatabase(Config.DatabaseName);
                MessageBox.Show(status.OperationMessage);
            }
        }

        private void CanAddCategoryItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OnAddCategoryItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Category cat = new Category();
            Category cc = e.Parameter as Category;
            if (cc != null)
                cat.Parent = cc;
            OperationStatus<Category> result1 = CategoryDetails.ShowWindow(cat);

            if (result1.Result == OperationResult.Passed)
            {
                var result = BooksManager.BooksManager.InsertCategory(result1.Data);

                if (result.Result == OperationResult.Passed)
                    RefreshCategoryList();
                else
                    MessageBox.Show(result.OperationMessage);
            }
        }

        private void CanRemoveCategoryItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CanRefreshCategoryListExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OnRefreshCategoryListExecute(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshCategoryList();
        }

        private void OnRemoveListItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void CanEditCategoryItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Category cat = e.Parameter as Category;
            e.CanExecute = (cat != null);
            e.Handled = true;
        }

        private void OnEditCategoryItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Category cat = e.Parameter as Category;
            if (cat != null)
            {
                OperationStatus<Category> result1 = CategoryDetails.ShowWindow(cat);

                if (result1.Result == OperationResult.Passed)
                {
                    var result = BooksManager.BooksManager.UpdateCategory(cat);

                    if (result.Result == OperationResult.Passed)
                        RefreshCategoryList();
                    else
                        MessageBox.Show(result.OperationMessage);
                }
            }
        }

        private void uxCategoryList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Category cat = e.NewValue as Category;
            if (cat != null)
            {
                selectedCategoryId = cat.Id;
                RefreshBooksList(new BookFilter() { RootCategoryId = selectedCategoryId });
                OrderByTitle();
            }
        }

        private void CanAddBookToCategoryItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Category cat = e.Parameter as Category;
            e.CanExecute = (cat != null);
            e.Handled = true;
        }

        private void OnAddBookToCategoryItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Category cat = e.Parameter as Category;
            if (cat != null)
            {
                Book book = new Book() { Category = cat };
                OperationStatus<Book> result1 = BookDetails.ShowWindow(this, book);

                if (result1.Result == OperationResult.Passed)
                {
                    var result = BooksManager.BooksManager.InsertBook(book);

                    if (result.Result == OperationResult.Passed)
                    {
                        RefreshBooksList(new BookFilter() { RootCategoryId = selectedCategoryId });

                        uxBooksList.CurrentItem = book;
                    }
                    else
                        MessageBox.Show(result.OperationMessage);
                }
            }
        }

        private void CanSearchBookItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OnSearchBookItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            RefreshBooksList(new BookFilter() { Title = uxSearchBox.Text, Author = uxSearchBox.Text, AdditionalInfo = uxSearchBox.Text, ISBN = uxSearchBox.Text });
        }

        private void CanChangeSkinItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OnChangeSkinItemExecute(object sender, ExecutedRoutedEventArgs e)
        {

            var app = Application.Current;

            Application.Current.Resources.Clear();

            ComboBoxItem item = CurrentSkinComboBox.SelectedItem as ComboBoxItem;
            
            Uri rd1 = new Uri("/BooksHouse;component/"+item.Tag.ToString(), UriKind.RelativeOrAbsolute);
            //Uri rd2 = new Uri("SecondDictionary.xaml", UriKind.Relative);
            //app.Resources.MergedDictionaries.Add(Application.LoadComponent(rd1) as ResourceDictionary);

            ResourceDictionary dictionary = Application.LoadComponent(rd1) as ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Add(dictionary);


            //             <ResourceDictionary Source="Themes/Calm-theme/Calm-theme.xaml"/>
            //<ResourceDictionary Source="Themes/Night-flowers-theme/Night-flowers-theme.xaml"/>  
        }


        private void RefreshBooksList(BookFilter filter)
        {
            uxBooksList.ItemsSource = BooksManager.BooksManager.GetBooksList(filter);
        }

        private void uxBooksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditBook();
        }

        private void EditBook()
        {
            Book book = uxBooksList.CurrentItem as Book;
            if (book != null)
            {
                //TODO wyslac kopie do okna tam zapisac jak ok
                OperationStatus<Book> result1 = BookDetails.ShowWindow(this, book);

                if (result1.Result == OperationResult.Passed)
                {
                    var result = BooksManager.BooksManager.UpdateBook(book);

                    if (result.Result == OperationResult.Passed)
                        RefreshBooksList(new BookFilter() { RootCategoryId = selectedCategoryId });
                    else
                        MessageBox.Show(result.OperationMessage);

                }
            }
        }
        private void OrderByAuthor()
        {
            uxBooksList.Items.SortDescriptions.Clear();

            var column = uxBooksList.Columns[0];
            uxBooksList.Items.SortDescriptions.Add(new SortDescription("Author", ListSortDirection.Ascending));

            applySortDirection(uxBooksList, column, ListSortDirection.Ascending);

            uxBooksList.Items.Refresh();
        }

        private void OrderByTitle()
        {
            uxBooksList.Items.SortDescriptions.Clear();

            var column = uxBooksList.Columns[0];
            uxBooksList.Items.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));

            applySortDirection(uxBooksList, column, ListSortDirection.Ascending);

            uxBooksList.Items.Refresh();
        }

        private void applySortDirection(DataGrid grid, DataGridColumn col, ListSortDirection listSortDirection)
        {
            foreach (DataGridColumn c in grid.Columns)
            {
                c.SortDirection = null;
            }
            col.SortDirection = listSortDirection;
        }

        private void SortByTitle_Click(object sender, RoutedEventArgs e)
        {
            OrderByTitle();
        }

        private void CanAddBookItemExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrWhiteSpace(uxAddBookTitle.Text) &&
                           !String.IsNullOrWhiteSpace(uxAddBookAuthor.Text) &&
                           !String.IsNullOrWhiteSpace(uxAddBookISBN.Text);
            e.Handled = true;
        }

        private void OnAddBookItemExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Book book = new Book();
            book.CategoryId = selectedCategoryId;
            book.Title = uxAddBookTitle.Text;
            book.Author = uxAddBookAuthor.Text;
            book.ISBN = uxAddBookISBN.Text;

            var result = BooksManager.BooksManager.InsertBook(book);

            if (result.Result == OperationResult.Passed)
            {
                uxAddBookTitle.Text = String.Empty;
                uxAddBookAuthor.Text = String.Empty;
                uxAddBookISBN.Text = String.Empty;
                RefreshBooksList(new BookFilter() { RootCategoryId = selectedCategoryId });
                var x = (uxBooksList.ItemsSource as IList<Book>).FirstOrDefault(b => b.Id == book.Id);
                uxBooksList.CurrentItem = x;
                uxBooksList.SelectedItem = x;
                uxBooksList.UpdateLayout();
                uxBooksList.ScrollIntoView(x);
            }
            else
                MessageBox.Show(result.OperationMessage);
        }

        private void SortByAuthor_Click(object sender, RoutedEventArgs e)
        {
            OrderByAuthor();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditBook();
        }

        private void uxSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
                RefreshBooksList(new BookFilter() { Title = uxSearchBox.Text, Author = uxSearchBox.Text, AdditionalInfo = uxSearchBox.Text, ISBN = uxSearchBox.Text });
        }

        
    }

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
