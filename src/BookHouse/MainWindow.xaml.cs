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
            uxCategoryList.ItemsSource = BooksManager.BooksManager.GetCategoryList(Constants.ROOT_CATEGORY);
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
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Clear();
            ComboBoxItem item = CurrentSkinComboBox.SelectedItem as ComboBoxItem;

            if (item != null)
            {
                Uri rd1 = new Uri("/BookHouse;component/"+item.Tag, UriKind.RelativeOrAbsolute);
                ResourceDictionary dictionary = Application.LoadComponent(rd1) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
        }

        private void RefreshBooksList(BookFilter filter)
        {
            uxBooksList.ItemsSource = BooksManager.BooksManager.GetBooksList(filter);
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

        private void OrderByTitle_Click(object sender, RoutedEventArgs e)
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

        private void OrderByAuthor_Click(object sender, RoutedEventArgs e)
        {
            OrderByAuthor();
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            EditBook();
        }

        private void uxSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
                RefreshBooksList(new BookFilter { Title = uxSearchBox.Text, Author = uxSearchBox.Text, AdditionalInfo = uxSearchBox.Text, ISBN = uxSearchBox.Text });
        }

        
    }
}
