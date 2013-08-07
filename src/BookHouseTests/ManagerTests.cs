using System;
using System.Drawing;
using System.Linq;
using BookHouse.BooksManager;
using BookHouse.Domain;
using BooksHouse.Domain;
using NUnit.Framework;

namespace BookHouseTests
{
    [SetUpFixture]
    public class TestsSetup
    {
        private const string database = "database_test.db";

        [SetUp]
        public void DataSetup()
        {
            BooksManager.CraeteDatabase(database);
            Config.DatabaseName = database;

            #region insert data

            //categories
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (1,0,'category 1')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (2,0,'category 2')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (3,1,'category 1.1')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (4,1,'category 1.2')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (5,1,'category to delete 1.3')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (6,3,'category 1.1.3')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (7,2,'category 2.1')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (8,2,'category 2.1')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (9,7,'category 2.1')");
            BooksManager.RunSQL("INSERT INTO CATEGORY VALUES (10,9,'category 2.1')");

            //books
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (1,NULL,'title 1','author 1','ISBN1','123','123',1,NULL)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (2,1,'title 2','author 2','234','234','123',1,NULL)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (3,3,'title 3','author 3','345','345','123',1,NULL)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (4,3,'title 4','author 4','456','456','123',1,NULL)");
            BooksManager.RunSQL(
                "INSERT INTO BOOK VALUES (5,5,'title 5 - to delete','author 5','567','567','123',1,NULL)");
            BooksManager.RunSQL(
                "INSERT INTO BOOK VALUES (6,5,'title 5 - category to delete','author 5','678','678','123',1,NULL)");

            #endregion
        }
    }

    [TestFixture]
    public class ManagerTests
    {
        [Test]
        public void ShouldGetCategory()
        {
            Category cat = BooksManager.GetCategory(1);

            Assert.AreEqual(1, cat.Id);
            Assert.AreEqual(0, cat.ParentId);
            Assert.AreEqual("category 1", cat.Name);


            Category cat2 = BooksManager.GetCategory(6);
            Assert.AreEqual(6, cat2.Id);
            Assert.AreEqual(3, cat2.ParentId);
            Assert.AreEqual("category 1.1.3", cat2.Name);
        }

        [Test]
        public void ShouldAddCategory()
        {
            Category cat = new Category { ParentId = 1, Name = "new category" };
            BooksManager.InsertCategory(cat);
            Assert.Greater(cat.Id, 0);
        }

        [Test]
        public void ShouldUpdateCategory()
        {
            Category cat = BooksManager.GetCategory(1);
            cat.Name = "edited";
            cat.ParentId = 3;
            BooksManager.UpdateCategory(cat);
            cat = BooksManager.GetCategory(1);

            Assert.AreEqual(3, cat.ParentId);
            Assert.AreEqual("edited", cat.Name);
        }

        [Test]
        public void ShouldDeleteCategory()
        {
            Category cat = BooksManager.GetCategory(5);
            Assert.AreEqual(5, cat.Id);
            BooksManager.DeleteCategory(cat);
            cat = BooksManager.GetCategory(5);
            Assert.IsNull(cat);

            Book bookWithNullCategory = BooksManager.GetBook(6);
            Assert.AreEqual(bookWithNullCategory.CategoryId, 0);
        }

        [Test]
        public void ShouldReadAllCategoryList()
        {
            var list = BooksManager.GetCategoryList(Constants.ROOT_CATEGORY, true);

            foreach (var category in list.Where(c => c.ParentId > 0))
                Assert.IsTrue(category.ParentId == category.Parent.Id);

            //parent of cat=2 is 0.....
            //foreach (var category in list.Where(c => c.ParentId == 0))
            //    Assert.IsNull(category.Parent);
        }

        [Test]
        public void ShouldReadHierachicalCategoryList()
        {
            var list = BooksManager.GetCategoryList(2, true);

            foreach (var category in list.Where(c => c.ParentId > 0))
                Assert.IsTrue(category.ParentId == category.Parent.Id);

            //parent of cat=2 is 0.....
            //foreach (var category in list.Where(c => c.ParentId == 0))
            //    Assert.IsNull(category.Parent);

            //should return 1 (category 2)
            Assert.AreEqual(1, list.Count);

            //should return 2 (2 subcategories of "category 2")
            Assert.AreEqual(2, list.First(c => c.Id == 2).SubCategories.Count);
        }

        [Test]
        public void ShouldReadBooks()
        {
            Book book1 = BooksManager.GetBook(1);
            Assert.AreEqual(1, book1.Id);
            Assert.AreEqual(0, book1.CategoryId);
            Assert.AreEqual("title 1", book1.Title);
            Assert.AreEqual("author 1", book1.Author);


            Book book2 = BooksManager.GetBook(2);
            Assert.AreEqual(2, book2.Id);
            Assert.AreEqual(1, book2.CategoryId);
            Assert.AreEqual("title 2", book2.Title);
            Assert.AreEqual("author 2", book2.Author);

            Book book3 = BooksManager.GetBook(3);
            Assert.AreEqual(3, book3.Id);
            Assert.AreEqual(3, book3.CategoryId);
            Assert.AreEqual("title 3", book3.Title);
            Assert.AreEqual("author 3", book3.Author);
        }

        [Test]
        public void ShouldAddBook()
        {
            Book book = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = "222222"
                };
            BooksManager.InsertBook(book);
            Assert.Greater(book.Id, 0);
        }

        [Test]
        public void BooksInDeletedCategoryShouldBeVisibleInMainCategory()
        {
            string isbn = "347563487562347856";
            Category newCategory = new Category
            {
                Name = "Category to delete",
                Parent = BooksManager.GetCategoryList(Constants.ROOT_CATEGORY, false)[0]
            };

            var result = BooksManager.InsertCategory(newCategory);

            Book newBook = new Book
                {
                    CategoryId = result.Data.Id,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = isbn
                };

            BooksManager.InsertBook(newBook);

            BooksManager.DeleteCategory(newCategory);
            var books = BooksManager.GetBooksList(new BookFilter { ISBN = isbn });
            Assert.AreEqual(books.Count, 1);
            Assert.IsNull(books[0].Category); 
        }


        [Test]
        public void ShouldAddBookWithCover()
        {
            Book book = new Book
                {
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

            BooksManager.InsertBook(book);
            Assert.Greater(book.Id, 0);

            var bookFromDatabase = BooksManager.GetBook(book.Id);
            Assert.IsNotNull(bookFromDatabase.Cover);
        }

        [Test]
        public void ShouldAddAndRemoveCoverFromBook()
        {
            Book book = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = "222226"
                };


            Bitmap bitmap = new Bitmap(4, 4);
            for (int x = 0; x < bitmap.Height; ++x)
                for (int y = 0; y < bitmap.Width; ++y)
                    bitmap.SetPixel(x, y, Color.Red);

            book.Cover = bitmap;

            var status = BooksManager.InsertBook(book);
            Assert.Greater(book.Id, 0);
            Assert.IsNotNull(status.Data.Cover);

            status.Data.Cover = null;
            var status2 = BooksManager.UpdateBook(book);
            Assert.AreEqual(status2.Result, OperationResult.Passed);

            var bookWithNoCover = BooksManager.GetBook(book.Id);
            Assert.IsNull(bookWithNoCover.Cover);
        }

        [Test]
        public void ShouldUpdateBook()
        {
            Book book = BooksManager.GetBook(4);
            book.Title = "edited";
            book.Author = "grzes";
            book.ISBN = "0909090909";
            book.AdditionalInfoLine1 = "additional";
            book.CategoryId = 3;
            BooksManager.UpdateBook(book);
            book = BooksManager.GetBook(4);

            Assert.AreEqual(3, book.CategoryId);
            Assert.AreEqual("edited", book.Title);
            Assert.AreEqual("grzes", book.Author);
            Assert.AreEqual("0909090909", book.ISBN);
            Assert.AreEqual("additional", book.AdditionalInfoLine1);
        }

        [Test]
        public void ShouldAddTwoBooksWithEmptyISBN()
        {
            Book book = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info"
                };
            var operation = BooksManager.InsertBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Passed);

            Book book2 = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info"
                };
            operation = BooksManager.InsertBook(book2);
            Assert.AreEqual(operation.Result, OperationResult.Passed);
        }

        [Test]
        public void ShouldUpdateBookWithEmptyISBN()
        {
            Book book = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = "ISBN13434"
                };
            var operation = BooksManager.InsertBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Passed);

            book.ISBN = String.Empty;
            operation = BooksManager.UpdateBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Passed);
        }

        [Test]
        public void ShouldNotAddBookWithExistingISBN()
        {
            Book book = new Book
                {
                    CategoryId = 1,
                    Title = "Book title",
                    Author = "pawel",
                    AdditionalInfoLine1 = "Additional Info",
                    ISBN = "ISBN1"
                };
            var operation = BooksManager.InsertBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Failed);
        }

        [Test]
        public void ShouldNotUpdateBookWithExistingISBN()
        {
            Book book = BooksManager.GetBook(4);
            string originalISBN = book.ISBN;
            book.ISBN = "ISBN1";
            var operation = BooksManager.InsertBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Failed);
            book = BooksManager.GetBook(4);
            Assert.AreEqual(originalISBN, book.ISBN);
        }

        [Test]
        public void ShouldDeleteBook()
        {
            Book book = BooksManager.GetBook(5);
            Assert.AreEqual(5, book.Id);
            BooksManager.DeleteBook(book);
            book = BooksManager.GetBook(5);
            Assert.IsNull(book);
        }

        [Test]
        public void ShouldFindManyBookInSearch()
        {
            var list = BooksManager.GetBooksList(new BookFilter());
            Assert.Greater(list.Count, 0);
        }

        [Test]
        public void ShouldFindOneBookInSearch()
        {
            var list = BooksManager.GetBooksList(new BookFilter { Author = "author 2" });
            Assert.AreEqual(list.Count, 1);
        }
    }
}