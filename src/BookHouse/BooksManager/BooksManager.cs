using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using BooksHouse.Domain;
using System.Linq;

namespace BooksHouse.BooksManager
{
    public class BooksManager
    {
        private const string connectionString = "Data Source={0};Version=3;New=False;Compress=True";
        private const string createCategoryTable = "CREATE TABLE CATEGORY (id INTEGER PRIMARY KEY, parent_id INTEGER NULL, name TEXT)";
        private const string createBooksTable = "CREATE TABLE BOOK (id INTEGER PRIMARY KEY, category_id INTEGER NULL, title TEXT, author TEXT, isbn TEXT UNIQUE, additionalInfo TEXT, entryDate INTEGER,photo BLOB)";

        public static OperationStatus<bool> CraeteDatabase(string filename)
        {
            OperationStatus<bool> status = new OperationStatus<bool>
                                               {
                                                   OperationMessage = "Database " + filename + " was created.",
                                                   Result = OperationResult.Passed
                                               };
            try
            {
                SQLiteConnection.CreateFile(filename);

                #region create tables
                SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, filename));
                connection.Open();
                using (SQLiteTransaction mytransaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                    {
                        mycommand.CommandText = createCategoryTable;
                        mycommand.ExecuteNonQuery();
                        mycommand.CommandText = createBooksTable;
                        mycommand.ExecuteNonQuery();
                    }
                    mytransaction.Commit();
                }
                connection.Close();
                #endregion
            }
            catch (Exception exc)
            {
                status.Result = OperationResult.Failed;
                status.OperationMessage = exc.Message;
            }

            return status;
        }

        public static OperationStatus<Book> InsertBook(Book book)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            if (!CheckUniqueISBN(connection, book.ISBN, 0))
                return new OperationStatus<Book> { OperationMessage = "Podany ISBN jest pusty lub jest ju¿ w bazie danych", Result = OperationResult.Failed, Data = book };

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    mycommand.CommandText = "INSERT INTO book (category_id, title, author, isbn, additionalInfo, entryDate) values(@categoryId, @title, @author, @isbn, @additionalInfo, @entryDate)";

                    mycommand.Parameters.AddWithValue("@categoryId", book.CategoryId);
                    mycommand.Parameters.AddWithValue("@title", book.Title);
                    mycommand.Parameters.AddWithValue("@author", book.Author);
                    mycommand.Parameters.AddWithValue("@isbn", book.ISBN);
                    mycommand.Parameters.AddWithValue("@additionalInfo", book.AdditionalInfo);
                    mycommand.Parameters.AddWithValue("@entryDate", Helpers.ConvertToUnixTimestamp(DateTime.Now));
                    mycommand.ExecuteNonQuery();

                    mycommand.CommandText = @"select last_insert_rowid()";
                    long lastId = (long)mycommand.ExecuteScalar();

                    book.Id = lastId;

                }
                mytransaction.Commit();
            }
            connection.Close();

            return new OperationStatus<Book> { OperationMessage = "Ksi¹¿ka zosta³a dodana.", Result = OperationResult.Passed, Data = book };
        }

        private static bool CheckUniqueISBN(SQLiteConnection connection, string isbn, long bookId)
        {
            long count;

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = "SELECT count(*) FROM book where isbn=@isbn";

                if (bookId > 0) mycommand.CommandText += " and id<>@id";
                mycommand.Parameters.AddWithValue("@isbn", isbn);
                if (bookId > 0) mycommand.Parameters.AddWithValue("@id", bookId);

                count = (long)mycommand.ExecuteScalar();
            }
            return count == 0;
        }

        public static OperationStatus<Book> UpdateBook(Book book)
        {
            byte[] photo = new byte[0];
            if (book.Cover != null)
                using (MemoryStream ms = new MemoryStream())
                {
                    book.Cover.Save(ms, book.Cover.RawFormat);
                    photo = ms.ToArray();
                }

            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            if (!CheckUniqueISBN(connection, book.ISBN, book.Id))
                return new OperationStatus<Book> { OperationMessage = "Podany ISBN jest pusty lub jest ju¿ w bazie danych", Result = OperationResult.Failed, Data = book };

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    mycommand.CommandText = "UPDATE book SET category_id=@categoryId, title=@title, author=@author,isbn=@isbn, additionalInfo=@additionalInfo, photo=@photo where id=@id";
                    mycommand.Parameters.Add("@photo", DbType.Binary, 20).Value = photo;
                    mycommand.Parameters.AddWithValue("@categoryId", book.CategoryId);
                    mycommand.Parameters.AddWithValue("@title", book.Title);
                    mycommand.Parameters.AddWithValue("@author", book.Author);
                    mycommand.Parameters.AddWithValue("@isbn", book.ISBN);
                    mycommand.Parameters.AddWithValue("@additionalInfo", book.AdditionalInfo);
                    mycommand.Parameters.AddWithValue("@id", book.Id);

                    mycommand.ExecuteNonQuery();
                }
                mytransaction.Commit();
            }
            connection.Close();

            return new OperationStatus<Book> { OperationMessage = "Ksi¹¿ka zosta³a zaktualizowana.", Result = OperationResult.Passed, Data = book };
        }

        public static OperationStatus<bool> DeleteBook(Book book)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    mycommand.CommandText = "DELETE FROM book where id=@id";

                    mycommand.Parameters.AddWithValue("@id", book.Id);

                    mycommand.ExecuteNonQuery();
                }
                mytransaction.Commit();
            }
            connection.Close();

            return new OperationStatus<bool>();
        }

        public static OperationStatus<Category> InsertCategory(Category category)
        {
            var status = new OperationStatus<Category>
                             {
                                 Data = category,
                                 OperationMessage = "Kategoria zosta³a dodana.",
                                 Result = OperationResult.Passed
                             };
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    mycommand.CommandText = "INSERT INTO category (parent_id,name) values(@parentId,@name)";

                    mycommand.Parameters.AddWithValue("@parentId", category.ParentId);
                    mycommand.Parameters.AddWithValue("@name", category.Name);

                    mycommand.ExecuteNonQuery();

                    mycommand.CommandText = @"select last_insert_rowid()";
                    long lastId = (long)mycommand.ExecuteScalar();

                    category.Id = lastId;

                }
                mytransaction.Commit();
            }
            connection.Close();

            return status;
        }

        public static OperationStatus<Category> UpdateCategory(Category category)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {
                    mycommand.CommandText = "UPDATE category SET parent_id=@parentId,name=@name where id=@id";

                    mycommand.Parameters.AddWithValue("@parentId", category.ParentId);
                    mycommand.Parameters.AddWithValue("@name", category.Name);
                    mycommand.Parameters.AddWithValue("@id", category.Id);

                    mycommand.ExecuteNonQuery();
                }
                mytransaction.Commit();
            }
            connection.Close();

            return new OperationStatus<Category> { OperationMessage = "Kategoria zosta³a zaktualizowana", Result = OperationResult.Passed, Data = category };
        }

        public static OperationStatus<bool> DeleteCategory(Category category)
        {
            //TODO delete child categories in recursive query

            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                SetCategoryToNullInBookTable(category.Id, connection);
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {

                    mycommand.CommandText = "DELETE FROM category where id=@id";

                    mycommand.Parameters.AddWithValue("@id", category.Id);

                    mycommand.ExecuteNonQuery();
                }
                mytransaction.Commit();
            }
            connection.Close();

            return new OperationStatus<bool> { OperationMessage = "Kategoria zosta³a usuniêta", Result = OperationResult.Passed, };
        }

        private static void SetCategoryToNullInBookTable(long id, SQLiteConnection connection)
        {
            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = "UPDATE book set category_id=NULL where category_id=@id";

                mycommand.Parameters.AddWithValue("@id", id);

                mycommand.ExecuteNonQuery();
            }

        }

        public static void RunSQL(string sql)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();
            using (SQLiteTransaction mytransaction = connection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                {

                    mycommand.CommandText = sql;

                    mycommand.ExecuteNonQuery();

                }
                mytransaction.Commit();
            }
            connection.Close();
        }

        public static Book GetBook(long id)
        {
            Book book = new Book();
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();


            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {

                mycommand.CommandText = @"SELECT c.name as CategoryName, b.id as Id, b.category_id as CategoryId, b.title as Title, b.author as Author, b.isbn as isbn, b.additionalInfo as additionalInfo, b.entryDate as entryDate, b.photo as cover from book b
Left outer join category c on c.id=b.category_id
where b.id=@id";
                // mycommand.CommandText = "SELECT b.id as Id, b.category_id as CategoryId, b.title as Title, b.author as Author, b.isbn as isbn, b.additionalInfo as additionalInfo from book b where b.id=@id";

                mycommand.Parameters.AddWithValue("@id", id);

                IDataReader reader = mycommand.ExecuteReader();
                DataTable table = new DataTable();

                table.Load(reader);

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        book = CreateBookRecord(row);
                    }
                }
                else
                {
                    book = null;
                }
                reader.Close();
            }

            connection.Close();

            return book;
        }

        private static Book CreateBookRecord(DataRow row)
        {
            Book book = new Book { Id = Int64.Parse(row["Id"].ToString()) };

            if (row["CategoryId"] != DBNull.Value)
                book.CategoryId = Int64.Parse(row["CategoryId"].ToString());
            book.CategoryName = row["CategoryName"].ToString();
            book.Title = row["Title"].ToString();
            book.Author = row["Author"].ToString();
            book.ISBN = row["isbn"].ToString();
            book.AdditionalInfo = row["additionalInfo"].ToString();
            book.EntryDate = Helpers.ConvertFromUnixTimestamp(long.Parse(row["entryDate"].ToString()));
            
            var cover=(row["cover"]) as byte[];
            if (cover != null && cover.Length>0)
                book.Cover = Image.FromStream(new MemoryStream((byte[])row["cover"]));

            return book;
        }

        public static Category GetCategory(int id)
        {
            Category category = new Category();
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = "SELECT cat.id as Id, cat.parent_id as ParentId, cat.name as Name from category cat where cat.id=@id";
                mycommand.Parameters.AddWithValue("@id", id);

                IDataReader reader = mycommand.ExecuteReader();
                DataTable table = new DataTable();

                table.Load(reader);

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        category.Id = Int64.Parse(row["Id"].ToString());

                        if (row["ParentId"] != DBNull.Value)
                            category.ParentId = Int64.Parse(row["ParentId"].ToString());

                        category.Name = row["Name"].ToString();
                    }
                }
                else
                {
                    category = null;
                }
                reader.Close();
            }

            connection.Close();

            return category;
        }

        public static IList<Category> GetCategoryList(long rootId)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();

            IList<Category> list = GetCategoryList(rootId, connection);
            connection.Close();
            return list;
        }

        public static IList<Category> GetCategoryList(long rootId, SQLiteConnection connection)
        {
            List<Category> list = new List<Category>();

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText = "SELECT cat.id as Id, cat.parent_id as ParentId, cat.name as Name from category cat ";//
                if (rootId > 0)
                {
                    mycommand.CommandText += "where cat.id=@id";
                    mycommand.Parameters.AddWithValue("@id", rootId);
                }

                IDataReader reader = mycommand.ExecuteReader();
                DataTable table = new DataTable();

                table.Load(reader);

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Category category = new Category { Id = Int64.Parse(row["Id"].ToString()) };

                        if (row["ParentId"] != DBNull.Value)
                            category.ParentId = Int64.Parse(row["ParentId"].ToString());

                        category.Name = row["Name"].ToString();
                        list.Add(category);
                    }
                }

                reader.Close();
                if (table.Rows.Count > 0)
                {
                    if (rootId > 0)
                    {
                        foreach (Category subCategory in list.ToList())
                            list.AddRange(GetSubCategories(subCategory.Id, connection));
                    }

                    foreach (var category in list.Where(cat => cat.Id > 0).ToList())
                    {
                        Category category1 = category;
                        category.Parent = list.FirstOrDefault(c => c.Id == category1.ParentId);
                        Category category2 = category;
                        category.SubCategories = list.Where(c => c.ParentId == category2.Id).ToList();
                        Category category3 = category;
                        list.RemoveAll(c => c.ParentId == category3.Id);
                    }
                }
                if (rootId == 0)
                    list.Insert(0, new Category { Id = 0, Name = "Categories (all books)", ParentId = 0 });
            }
            return list;
        }

        public static bool DatabaseFileExists(string databaseName)
        {
            return System.IO.File.Exists(databaseName);
        }

        public static IList<Book> GetBooksList(long selectedCategoryId)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();
            BookFilter filter = new BookFilter { RootCategoryId = selectedCategoryId };
            IList<Book> list = GetBooksList(filter, connection);
            connection.Close();

            return list;
        }

        public static IList<Book> GetBooksList(BookFilter filter)
        {
            SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
            connection.Open();
            IList<Book> list = GetBooksList(filter, connection);
            connection.Close();

            return list;
        }

        public static IList<Book> GetBooksList(BookFilter filter, SQLiteConnection connection)
        {
            List<Book> list = new List<Book>();

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText =
                    @"SELECT c.name as CategoryName, b.id as Id, b.category_id as CategoryId, b.title as Title, b.author as Author, b.isbn as isbn, b.additionalInfo as additionalInfo, b.entryDate as entryDate, b.photo as cover from book b
Left outer join category c on c.id=b.category_id
where b.category_id=@category_id";
                if (filter.HasTextFilter)
                {
                    mycommand.CommandText += " and (";
                    List<string> search_list = new List<string>();
                    if (!String.IsNullOrWhiteSpace(filter.Author))
                        search_list.Add(" upper(b.author) like upper(@author)");
                    if (!String.IsNullOrWhiteSpace(filter.Title))
                        search_list.Add(" upper(b.title) like upper(@title) ");
                    if (!String.IsNullOrWhiteSpace(filter.AdditionalInfo))
                        search_list.Add(" upper(b.additionalInfo) like upper(@additionalInfo) ");
                    if (!String.IsNullOrWhiteSpace(filter.ISBN))
                        search_list.Add(" upper(b.isbn) like upper(@isbn) ");

                    mycommand.CommandText += string.Join("or", search_list) + ")";
                }

                mycommand.Parameters.AddWithValue("@category_id", filter.RootCategoryId);

                if (!String.IsNullOrWhiteSpace(filter.Author))
                    mycommand.Parameters.AddWithValue("@author", "%" + filter.Author + "%");
                if (!String.IsNullOrWhiteSpace(filter.Title))
                    mycommand.Parameters.AddWithValue("@title", "%" + filter.Title + "%");
                if (!String.IsNullOrWhiteSpace(filter.AdditionalInfo))
                    mycommand.Parameters.AddWithValue("@additionalInfo", "%" + filter.AdditionalInfo + "%");
                if (!String.IsNullOrWhiteSpace(filter.ISBN))
                    mycommand.Parameters.AddWithValue("@isbn", "%" + filter.ISBN + "%");

                IDataReader reader = mycommand.ExecuteReader();
                DataTable table = new DataTable();

                table.Load(reader);

                if (table.Rows.Count > 0)
                {
                    list.AddRange(from DataRow row in table.Rows select CreateBookRecord(row));
                }

                reader.Close();
            }

            foreach (Category subCategory in GetSubCategories(filter.RootCategoryId, connection).ToList())
            {
                BookFilter newFilter = new BookFilter(filter) { RootCategoryId = subCategory.Id };
                list.AddRange(GetBooksList(newFilter, connection));
            }

            return list;
        }

        private static IEnumerable<Category> GetSubCategories(long parentCategoryId, SQLiteConnection connection)
        {
            List<Category> list = new List<Category>();

            using (SQLiteCommand mycommand = new SQLiteCommand(connection))
            {
                mycommand.CommandText =
                    "SELECT cat.id as Id, cat.parent_id as ParentId, cat.name as Name from category cat ";

                mycommand.CommandText += "where cat.parent_id=@id";
                mycommand.Parameters.AddWithValue("@id", parentCategoryId);


                IDataReader reader = mycommand.ExecuteReader();
                DataTable table = new DataTable();

                table.Load(reader);

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        Category category = new Category { Id = Int64.Parse(row["Id"].ToString()) };

                        if (row["ParentId"] != DBNull.Value)
                            category.ParentId = Int64.Parse(row["ParentId"].ToString());

                        category.Name = row["Name"].ToString();
                        list.Add(category);
                    }
                }

                reader.Close();
                if (table.Rows.Count > 0)
                {
                    foreach (Category subCategory in list.ToList())
                        list.AddRange(GetSubCategories(subCategory.Id, connection));
                }
            }
            return list;
        }

        public static OperationStatus<bool> UploadBookCover(Book book, Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                byte[] photo = ms.ToArray();

                SQLiteConnection connection = new SQLiteConnection(String.Format(connectionString, Config.DatabaseName));
                connection.Open();

                using (SQLiteTransaction mytransaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand mycommand = new SQLiteCommand(connection))
                    {
                        mycommand.CommandText = "UPDATE book SET photo=@photo where id=@id";
                        mycommand.Parameters.Add("@photo", DbType.Binary, 20).Value = photo;
                        mycommand.Parameters.AddWithValue("@id", book.Id);

                        mycommand.ExecuteNonQuery();
                    }
                    mytransaction.Commit();
                }
                connection.Close();
            }
            return new OperationStatus<bool> { OperationMessage = "Ok³adka zosta³a dodana", Result = OperationResult.Passed, };
        }

    }
}