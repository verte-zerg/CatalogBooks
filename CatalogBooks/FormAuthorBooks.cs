using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace CatalogBooks
{
    public partial class FormAuthorBooks : Form
    {
        /// <summary>
        /// ID выбранного автора
        /// </summary>
        private int AuthorId { get; }
        /// <summary>
        /// Объект для работы с базой данных
        /// </summary>
        private readonly BaseContext _db;

        /// <summary>
        /// Уменьшает длинну строки до подходящих размеров
        /// </summary>
        /// <param name="str">Исходная строка</param>
        /// <returns>Уменьшенная строка</returns>
        private string GetCutString(string str)
        {
            using (Graphics gr = dataGridViewBooks.CreateGraphics())
            {
                double width = gr.MeasureString(str, Font).Width;
                if (width > dataGridViewBooks.Width)
                    return str.Substring(0, Convert.ToInt32(str.Length * (dataGridViewBooks.Width)/width));
            }
            return str;
        }

        /// <summary>
        /// Выполняет загрузку данных
        /// </summary>
        private void LoadData()
        {
            var books = _db.Authors.Where(author => author.AuthorID == AuthorId).Select(author => author.Books).First();            
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(Int32));            
            dt.Columns.Add("Название", typeof(String));
            foreach (Book book in books)
                dt.Rows.Add(book.BookId, GetCutString(book.Name));
            dataGridViewBooks.DataSource = dt;
            dataGridViewBooks.Columns["ID"].Visible = false;
        }

        public FormAuthorBooks(int authorId)
        {
            InitializeComponent();
            AuthorId = authorId;
            _db = new BaseContext();
            TransparencyKey = dataGridViewBooks.BackgroundColor;
        }

        private void FormAuthorBooks_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void FormAuthorBooks_Deactivate(object sender, EventArgs e)
        {
            Close();
        }

        private void dataGridViewBooks_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewBooks.SelectedRows.Count > 0)
            {
                string path = _db.Books.Find(dataGridViewBooks.SelectedRows[0].Cells["ID"].Value).Path;
                if (File.Exists(path))
                    Process.Start(path);
                else
                    MessageBox.Show(String.Format("Файл по указанному пути не найден: {0}", path));
            }
        }
    }
}
