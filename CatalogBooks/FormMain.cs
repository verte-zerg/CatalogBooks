using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogBooks.Properties;

namespace CatalogBooks
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// Объект для работы с базой данных
        /// </summary>
        private readonly BaseContext _db;
        /// <summary>
        /// Показывает, активировалась ли форма
        /// </summary>
        private bool _activeForm;
        /// <summary>
        /// Объекты для храниния исходных данных таблиц
        /// </summary>
        private List<ListViewItem> _listBook, _listAuthor;        
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            _db = new BaseContext();                           
        }
        /// <summary>
        /// Заполнение списка книг
        /// </summary>
        private void FillListViewBooks()
        {
            _listBook = new List<ListViewItem>();   
            listViewBooks.Items.Clear();

            foreach (Book item in _db.Books)
            {
                ListViewItem listItem = new ListViewItem(item.BookId.ToString());
                string authors = "";

                var authorCollection =
                    _db.Books.Where(book => book.BookId == item.BookId).SelectMany(book => book.Authors);

                foreach (Author author in authorCollection)
                    authors += String.Format("{0}. {1}; ", author.FirstName[0], author.LastName);

                string name = item.Name;

                if (name.Length > 50)
                    name = name.Substring(0, 50) + "...";

                listItem.SubItems.AddRange(new[] { name, item.Year.ToString(), authors });
                _listBook.Add(listItem);               
            }            
            listViewBooks.Items.AddRange(_listBook.ToArray());

            listViewBooks.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewBooks.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        /// <summary>
        /// Заполнение списка авторов
        /// </summary>
        private void FillListViewAuthors()
        {
            _listAuthor = new List<ListViewItem>();
            listViewAuthors.Items.Clear();

            foreach (Author item in _db.Authors)
            {
                ListViewItem listItem = new ListViewItem(item.AuthorID.ToString());

                int bookCount = _db.Authors.Where(author => author.AuthorID == item.AuthorID).SelectMany(author => author.Books).Count();

                listItem.SubItems.AddRange(new [] { item.LastName, item.FirstName, bookCount.ToString() });
                _listAuthor.Add(listItem);
            }
            listViewAuthors.Items.AddRange(_listAuthor.ToArray());

            listViewAuthors.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewAuthors.AutoResizeColumn(3, ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void ScanFiles(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem) && Settings.Default.AutoScan)
                new FormBooksDetected().ShowDialog(); 
              
            if (sender is ToolStripMenuItem)
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
                string path = _db.Books.Find(int.Parse(listViewBooks.SelectedItems[0].SubItems[0].Text)).Path;
                if (File.Exists(path))
                    Process.Start(path);
                else
                    MessageBox.Show(String.Format("Файл по указанному пути не найден: {0}.", path), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listViewAuthors_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewAuthors.SelectedItems.Count > 0 && _activeForm)
            {
                FormAuthorBooks formAuthorBooks = new FormAuthorBooks(int.Parse(listViewAuthors.SelectedItems[0].SubItems[0].Text));                
                int x = listViewBooks.PointToScreen(Point.Empty).X + e.X;
                int y = listViewBooks.PointToScreen(Point.Empty).Y + e.Y;
                formAuthorBooks.DesktopLocation = new Point(x, y);    
                _activeForm = false;
                formAuthorBooks.Show();                
            }
        }

        private void listViewAuthors_MouseClick(object sender, MouseEventArgs e)
        {
            _activeForm = true;
        }

        private void toolStripTextBoxFilter_TextChanged(object sender, EventArgs e)
        {
            listViewBooks.Items.Clear();
            listViewAuthors.Items.Clear();

            if (toolStripTextBoxFilter.Text != "")
            {
                string search = toolStripTextBoxFilter.Text.ToLower();
                listViewBooks.Items.AddRange(_listBook.Where(x => x.SubItems[1].ToString().ToLower().Contains(search)).ToArray());

                listViewAuthors.Items.AddRange(_listAuthor.Where(x => x.SubItems[1].ToString().ToLower().Contains(search) 
                    || x.SubItems[2].ToString().ToLower().Contains(search)).ToArray());
            }
            else
            {
                listViewBooks.Items.AddRange(_listBook.ToArray());
                listViewAuthors.Items.AddRange(_listAuthor.ToArray());
            }

        }
    }
}