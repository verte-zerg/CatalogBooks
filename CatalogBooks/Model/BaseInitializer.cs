using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace CatalogBooks
{
    public class BaseInitializer : DropCreateDatabaseIfModelChanges<BaseContext>
    {
        protected override void Seed(BaseContext context)
        {
            var authors = new List<Author> 
            { 
                new Author { FirstName = "Михаил", LastName = "Флёнов" },
                new Author { FirstName = "Томас", LastName = "Кормен" },
                new Author { FirstName = "Рональд", LastName = "Ривест" }
            };
            authors.ForEach(s => context.Authors.Add(s));
            context.SaveChanges();

            var books = new List<Book> 
            { 
                new Book { Name = "Алгоритмы. Построение и анализ", Year = 2013, KeyWords = "алгоритмы;шедевр;", Authors = authors.ToList()},
                new Book { Name = "Библия C#", Year = 2008, KeyWords = "c#;" }
            };
            books.ForEach(s => context.Books.Add(s));
            context.SaveChanges();
        }
    }
}