using System;
using System.Collections.Generic;

namespace CatalogBooks
{
    public class Book
    {
        public int BookId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }        
        public virtual ICollection<Author> Authors { get; set; }
        public string KeyWords { get; set; }
        public string MD5 { get; set; }
        public string Path { get; set; }
    }
}