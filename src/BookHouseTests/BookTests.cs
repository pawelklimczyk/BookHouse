using System.Drawing;
using BookHouse.Domain;
using NUnit.Framework;

namespace BooksHouseTests
{
    [TestFixture]
    public class BookTests
    {
        [Test]
        public void ShouldCopyBook()
        {
            Book book = new Book
                {
                    Id = 123,
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = "222224"
                };

            Bitmap bitmap = new Bitmap(4, 4);
            for (int x = 0; x < bitmap.Height; ++x)
                for (int y = 0; y < bitmap.Width; ++y)
                    bitmap.SetPixel(x, y, Color.Red);
            book.Cover = bitmap;

            Book copy = book.Copy();

            foreach (var propertyInfo in typeof(Book).GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(Image))
                {
                    Bitmap img1 = (Bitmap)propertyInfo.GetValue(book, null);
                    Bitmap img2 = (Bitmap)propertyInfo.GetValue(copy, null);

                    for (int x = 0; x < img1.Height; ++x)
                        for (int y = 0; y < img1.Width; ++y)
                        {
                            Assert.AreEqual(img1.GetPixel(x, y), img2.GetPixel(x, y));
                        }
                }
                else
                {
                    Assert.AreEqual(propertyInfo.GetValue(book, null), propertyInfo.GetValue(copy, null));
                }
            }
        }
    }
}