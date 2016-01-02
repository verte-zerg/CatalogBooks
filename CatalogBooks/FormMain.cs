using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CatalogBooks.Properties;

namespace CatalogBooks
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// Объект для работы с базой данных
        /// </summary>
        private BaseContext db;
        /// <summary>
        /// Объекты для храниния исходных данных таблиц
        /// </summary>
        private List<ListViewItem> listBook, listAuthor;        
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            db = new BaseContext();                           
        }
        /// <summary>
        /// Заполнение списка книг
        /// </summary>
        private void FillListViewBooks()
        {
            listBook = new List<ListViewItem>();   
            listViewBooks.Items.Clear();         

            foreach (Book item in db.Books)
            {
                ListViewItem listItem = new ListViewItem(item.BookId.ToString());
                string authors = "";

                var authorCollection =
                    db.Books.Where(book => book.BookId == item.BookId).SelectMany(book => book.Authors);

                foreach (Author author in authorCollection)
                    authors += String.Format("{0}. {1}; ", author.FirstName[0], author.LastName);

                string name = item.Name.ToString();

                if (name.Length > 50)
                    name = name.Substring(0, 50) + "...";

                listItem.SubItems.AddRange(new string[] { name, item.Year.ToString(), authors });
                listBook.Add(listItem);               
            }            
            listViewBooks.Items.AddRange(listBook.ToArray());

            listViewBooks.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewBooks.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        /// <summary>
        /// Заполнение списка авторов
        /// </summary>
        private void FillListViewAuthors()
        {
            listAuthor = new List<ListViewItem>();
            listViewAuthors.Items.Clear();

            foreach (Author item in db.Authors)
            {
                ListViewItem listItem = new ListViewItem(item.AuthorID.ToString());

                int bookCount = db.Authors.Where(author => author.AuthorID == item.AuthorID).SelectMany(author => author.Books).Count();

                listItem.SubItems.AddRange(new string[] { item.LastName.ToString(), item.FirstName.ToString(), bookCount.ToString() });
                listAuthor.Add(listItem);
            }
            listViewAuthors.Items.AddRange(listAuthor.ToArray());

            listViewAuthors.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewAuthors.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ScanFiles(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem))
            {
                if (Settings.Default.AutoScan)
                    new FormBooksDetected().ShowDialog();   
            }
            else
                new FormBooksDetected().ShowDialog();

            FillListViewBooks();
            FillListViewAuthors();                  
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormSettings().ShowDialog();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showFormDBMenuItem_Click(object sender, EventArgs e)
        {
            new FormDataBase((sender as ToolStripMenuItem).Text).ShowDialog();
            
            FillListViewBooks();
            FillListViewAuthors();
        }

        private void listViewBooks_DoubleClick(object sender, EventArgs e)
        {
            if (listViewBooks.SelectedItems.Count > 0)
            {
                string path = db.Books.Find(int.Parse(listViewBooks.SelectedItems[0].SubItems[0].Text)).Path;
                if (File.Exists(path))
                    Process.Start(path);
                else
                    MessageBox.Show(String.Format("Файл по указанному пути не найден: {0}", path));
            }
        }

        private void listViewAuthors_DoubleClick(object sender, EventArgs e)
        {

        }

        private void toolStripTextBoxFilter_TextChanged(object sender, EventArgs e)
        {
            listViewBooks.Items.Clear();
            listViewAuthors.Items.Clear();

            if (toolStripTextBoxFilter.Text != "")
            {
                listViewBooks.Items.AddRange(listBook.Where(x => x.SubItems[1].ToString().IndexOf(toolStripTextBoxFilter.Text) != -1).ToArray());

                listViewAuthors.Items.AddRange(listAuthor.Where(x => x.SubItems[1].ToString().IndexOf(toolStripTextBoxFilter.Text) != -1 ||
                    x.SubItems[2].ToString().IndexOf(toolStripTextBoxFilter.Text) != -1).ToArray());
            }
            else
            {
                listViewBooks.Items.AddRange(listBook.ToArray());
                listViewAuthors.Items.AddRange(listAuthor.ToArray());
            }

        }
    }
}