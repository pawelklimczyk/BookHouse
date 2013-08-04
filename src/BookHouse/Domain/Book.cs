using System;
using System.ComponentModel;
using System.Drawing;

namespace BooksHouse.Domain
{
    public class Book : INotifyPropertyChanged
    {
        private Category category;

        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }

        public Category Category
        {
            get { return category; }
            set
            {
                category = value;

                if (category != null)
                {
                    CategoryId = category.Id;
                    CategoryName = category.Name;
                }
                else
                {
                    CategoryId = 0; CategoryName = string.Empty;
                }
            }
        }
        public string Title { get; set; }
        public string Author { get; set; }
        public string AdditionalInfoLine1 { get; set; }
        public string AdditionalInfoLine2 { get; set; }
        public string ISBN { get; set; }
        public DateTime EntryDate { get; set; }

        private Image cover;
        public Image Cover
        {
            get { return cover; }
            set
            {
                if (cover != value)
                {
                    cover = value;
                    RaisePropertyChanged("Cover");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Book Copy()
        {
            Book newBook =(Book) this.MemberwiseClone();
            if (this.Cover != null)
                newBook.Cover = (Image) this.Cover.Clone();

            return newBook;
        }
    }
}
