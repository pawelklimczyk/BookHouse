#region Using directives

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using BooksHouse.Domain;
using Microsoft.Win32;
using Image = System.Drawing.Image;

#endregion

namespace BooksHouse.Gui.Dialog
{
    public partial class BookDetails : Window
    {
        private Book book;
        private BookDetails()
        {
            InitializeComponent();
            KeyDown += HandleKeys;
            Closed += CleanUp;
        }

        private void CleanUp(object sender, EventArgs e)
        {
            KeyDown -= HandleKeys;
            Closed -= CleanUp;
        }

        private void HandleKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                Close();
            }
        }

        public static OperationStatus<Book> ShowWindow(Window owner, Book obj)
        {
            OperationStatus<Book> status = new OperationStatus<Book>();
            status.Data = obj;
            BookDetails window = new BookDetails();
            window.Owner = owner;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.DataContext = obj;
            window.book = obj;
            window.uxCategoryComboBox.ItemsSource = BooksManager.BooksManager.GetCategoryList(Constants.ROOT_CATEGORY);
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

        private void btn_selectCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".jpg";
            dialog.Filter = "JPG iamges (.jpg)|*.jpg";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                try
                {
                    Image photo = System.Drawing.Image.FromFile(dialog.FileName);

                    using (Bitmap scaled = new Bitmap((int)Constants.IMAGE_WIDTH, (int)Constants.IMAGE_HEIGHT, System.Drawing.Imaging.PixelFormat.Format48bppRgb))
                    {
                        using (Graphics graphics = Graphics.FromImage(scaled))
                        {
                            int scaledWidth = (photo.Width * (int)Constants.IMAGE_HEIGHT) / photo.Height;
                            if (scaledWidth < Constants.IMAGE_WIDTH)
                            {
                                graphics.DrawImage(photo, new Rectangle(0, 0, (int)Constants.IMAGE_WIDTH, (int)Constants.IMAGE_HEIGHT));
                            }
                            else
                            {
                                int diff = (int) (((scaledWidth - Constants.IMAGE_WIDTH) * (photo.Width/Constants.IMAGE_WIDTH))/2);
                                graphics.DrawImage(photo, new Rectangle(0, 0, scaledWidth, (int)Constants.IMAGE_HEIGHT), new Rectangle(diff, 0, photo.Width - diff, photo.Height), GraphicsUnit.Pixel);

                            }
                        }

                        MemoryStream MyStream = new MemoryStream();

                        scaled.Save(MyStream, ImageFormat.Jpeg);


                        book.Cover = Image.FromStream(MyStream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void btn_deleteCover_Click(object sender, RoutedEventArgs e)
        {
            book.Cover = null;
        }
    }
}
