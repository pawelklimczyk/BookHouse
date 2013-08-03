using System;
using System.Collections.Generic;

namespace BooksHouse.Domain
{
    public class Category
    {
        public Category()
        {
            SubCategories = new List<Category>();
        }
        public long Id { get; set; }
        public long ParentId { get; set; }
        private Category parent;
        public Category Parent
        {
            get { return parent; }
            set
            {
                parent = value;

                if (parent != null)
                {
                    ParentId = parent.Id;
                }
                else
                {
                    ParentId = 0;
                }
            }
        }

        public IList<Category> SubCategories { get; set; }
        public string Name { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Category cat = obj as Category;



            return Id == cat.Id;

        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}