using BooksHouse.Domain;
using MbUnit.Framework;
using BooksHouse.BooksManager;
using System.Linq;

namespace BooksHouseTests
{
    [TestFixture]
    public class ManagerTests
    {
        private const string database = "database_test.db";

        [FixtureSetUp]
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
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (1,NULL,'title 1','author 1','ISBN1','123',1)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (2,1,'title 2','author 2','234','234',1)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (3,3,'title 3','author 3','345','345',1)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (4,3,'title 4','author 4','456','456',1)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (5,5,'title 5 - to delete','author 5','567','567',1)");
            BooksManager.RunSQL("INSERT INTO BOOK VALUES (6,5,'title 5 - category to delete','author 5','678','678',1)");

            #endregion
        }

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
            Assert.GreaterThan(cat.Id, 0);
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
            var list = BooksManager.GetCategoryList(0);

            foreach (var category in list.Where(c => c.ParentId > 0))
                Assert.IsTrue(category.ParentId == category.Parent.Id);

            //parent of cat=2 is 0.....
            //foreach (var category in list.Where(c => c.ParentId == 0))
            //    Assert.IsNull(category.Parent);
        }

        [Test]
        public void ShouldReadHierachicalCategoryList()
        {
            var list = BooksManager.GetCategoryList(2);

            foreach (var category in list.Where(c => c.ParentId > 0))
                Assert.IsTrue(category.ParentId == category.Parent.Id);

            //parent of cat=2 is 0.....
            //foreach (var category in list.Where(c => c.ParentId == 0))
            //    Assert.IsNull(category.Parent);

            //should return 1 (category 2)
            Assert.AreEqual(1, list.Count);

            //should return 2 (2 subcategories of "category 2")
            Assert.AreEqual(2,list.First(c=>c.Id==2).SubCategories.Count);
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
            Book book = new Book { CategoryId = 1, Title = "Book title", Author = "pawel", AdditionalInfo = "Additional Info", ISBN = "222222" };
            BooksManager.InsertBook(book);
            Assert.GreaterThan(book.Id, 0);
        }

        [Test]
        public void ShouldUpdateBook()
        {
            Book book = BooksManager.GetBook(4);
            book.Title = "edited";
            book.Author = "grzes";
            book.ISBN = "0909090909";
            book.AdditionalInfo = "additional";
            book.CategoryId = 3;
            BooksManager.UpdateBook(book);
            book = BooksManager.GetBook(4);

            Assert.AreEqual(3, book.CategoryId);
            Assert.AreEqual("edited", book.Title);
            Assert.AreEqual("grzes", book.Author);
            Assert.AreEqual("0909090909", book.ISBN);
            Assert.AreEqual("additional", book.AdditionalInfo);
        }

        [Test]
        public void ShouldAddBookWithExistingISBN()
        {
            Book book = new Book { CategoryId = 1, Title = "Book title", Author = "pawel", AdditionalInfo = "Additional Info", ISBN = "ISBN1" };
            var operation = BooksManager.InsertBook(book);
            Assert.AreEqual(operation.Result, OperationResult.Failed);
        }

        [Test]
        public void ShouldUpdateBookWithExistingISBN()
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
            var list = BooksManager.GetBooksList(new BookFilter() );
            Assert.GreaterThan(list.Count,0);
        }
        [Test]
        public void ShouldFindOneBookInSearch()
        {
            var list = BooksManager.GetBooksList(new BookFilter { Author = "author 2" });
            Assert.AreEqual(list.Count, 1);
        }
    }
}
