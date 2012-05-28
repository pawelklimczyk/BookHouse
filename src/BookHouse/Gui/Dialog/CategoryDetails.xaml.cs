using System.Linq;
using System.Windows;
using System.Windows.Input;
using BooksHouse.Domain;

namespace BooksHouse.Gui.Dialog
{
    public partial class CategoryDetails : Window
    {
        private CategoryDetails()
        {
            InitializeComponent();
            KeyDown += HandleKeys;
        }

        private void HandleKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }

        public static OperationStatus<Category> ShowWindow(Category obj)
        {
            OperationStatus<Category> status = new OperationStatus<Category>();
            status.Data = obj;
            CategoryDetails window = new CategoryDetails();
            window.DataContext = obj;
            window.uxCategoryComboBox.ItemsSource = BooksManager.BooksManager.GetCategoryList(Constants.ROOT_CATEGORY).Where(c => c.Id != obj.Id);

            var retStatus = window.ShowDialog();

            if (retStatus.HasValue && retStatus.Value)
            {
                status.Result = OperationResult.Passed;
            }
            else
                status.Result = OperationResult.Failed;

            return status;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btn_accept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

    }
}
