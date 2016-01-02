using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogBooks
{
    /// <summary>
    /// Структура описания дополнительной информации
    /// </summary>
    public struct AdditionalInfo
    {
        public string Authors { get; set; }
        public string Keywords { get; set; }        
    }

    public partial class FormBooksDetected : Form
    {
        /// <summary>
        /// Объект для хранения обнаруженных файлов
        /// </summary>
        private List<Book> ListBooks { get; set; }
        /// <summary>
        /// Объект для хранения дополнительной инфорамации об обнаруженных файлов
        /// </summary>
        private Dictionary<string, AdditionalInfo> DictAuthor { get; set; }
        /// <summary>
        /// Объект для работы с базой данных
        /// </summary>
        private BaseContext db;
        /// <summary>
        /// Объект для хранинения отображаемой информации о файлах
        /// </summary>
        private DataTable dt;
        /// <summary>
        /// Объект для сканирования папок
        /// </summary>
        private CheckFiles checkFiles;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public FormBooksDetected()
        {
            InitializeComponent();
            db = new BaseContext();
            db.Books.Load();
            db.Authors.Load();
            DictAuthor = new Dictionary<string, AdditionalInfo>();
            dt = new DataTable("Books");
            checkFiles = new CheckFiles(".pdf|.djvu|.fb2");
            ListBooks = new List<Book>();
        }

        /// <summary>
        /// Загрузка данных в DataTable
        /// </summary>
        private void LoadData()
        {           
            dt.Columns.Add("Добавить", Type.GetType("System.Boolean"));
            dt.Columns.Add("Название", Type.GetType("System.String"));
            dt.Columns.Add("Год издания", Type.GetType("System.Int32"));
            dt.Columns.Add("Путь", Type.GetType("System.String"));            

            foreach (Book book in ListBooks)
            {
                string name = Path.GetFileName(book.Path);
                name = name.Remove(name.LastIndexOf('.'));
                dt.Rows.Add(true, name, 2000, book.Path);
                DictAuthor[book.Path] = new AdditionalInfo()
                {
                    Authors = "",
                    Keywords = ""
                };
            }
            dataGridViewMain.DataSource = dt;

            dataGridViewMain.Columns["Путь"].ReadOnly = true;
        }

        /// <summary>
        /// Сохранение изменений а базе данных
        /// </summary>
        private void SaveData()
        {
            for (int i = 0; i < ListBooks.Count; i++)
            {
                ListBooks[i].Name = dt.Rows[i].ItemArray[1].ToString();
                ListBooks[i].Year = (int) dt.Rows[i].ItemArray[2];
            }

            for (int i = 0; i < ListBooks.Count; i++)
                if ((bool) dt.Rows[i].ItemArray[0])
                {
                    ListBooks[i].KeyWords = DictAuthor[ListBooks[i].Path].Keywords;
                    db.Books.Add(ListBooks[i]);

                    string authors = DictAuthor[ListBooks[i].Path].Authors;

                    if (authors == "")
                        authors = "noname noname";

                    foreach (string authorInfo in authors.Split(';'))
                    {
                        string firstName = authorInfo.Substring(authorInfo.IndexOf(' ') + 1);
                        string lastName = authorInfo.Substring(0, authorInfo.IndexOf(' '));
                        if (!db.Authors.Any(x => x.FirstName == firstName && x.LastName == lastName))
                            db.Authors.Add(new Author()
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Books = new List<Book>() {ListBooks[i]}
                            });
                        else
                        {
                            Author author =
                                db.Authors.Where(x => x.FirstName == firstName && x.LastName == lastName).First();
                            author.Books.Add(ListBooks[i]);
                        }
                    }

                    db.SaveChanges();
                }
        }

        /// <summary>
        /// Снимает/утанавливает флажки во всей таблице
        /// </summary>
        /// <param name="check">Устанавлиаемое значение</param>
        private void CheckedItems(bool check)
        {
            foreach (DataGridViewRow row in dataGridViewMain.Rows)
                row.Cells["Добавить"].Value = check;
        }

        /// <summary>
        /// Выполняет сканирование папок
        /// </summary>
        /// <returns></returns>
        private async Task CheckDirectory()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (string path in Properties.Settings.Default.PathsScan.Split(';'))
                if (path != "")
                {
                    toolStripStatusLabelInfo.Text = String.Format("Проверка каталога: {0}", path);
                    ListBooks.AddRange(await checkFiles.Check(path));
                }
            sw.Stop();

            TimeSpan elapsedTime = sw.Elapsed;

            if (ListBooks.Count == 0)
                toolStripStatusLabelInfo.Text = "Сканирование не дало результатов. ";
            else
                toolStripStatusLabelInfo.Text = String.Format("Было обнаружено: {0} файлов. ", ListBooks.Count);

            toolStripStatusLabelInfo.Text += String.Format("Время: {0:F2} сек.", elapsedTime.TotalSeconds);            

            LoadData();
        } 

        private async void FormBooksDetected_Load(object sender, EventArgs e)
        {
            await CheckDirectory();
        }

        private void dataGridViewMain_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            DictAuthor[dataGridViewMain.Rows[e.RowIndex].Cells["Путь"].Value.ToString()] = new AdditionalInfo()
            {
                Authors = textBoxAuthors.Text,
                Keywords = textBoxKeywords.Text
            };
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            SaveData();
            Close();
        }

        private void dataGridViewMain_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            AdditionalInfo additionalInfo = DictAuthor[dataGridViewMain.Rows[e.RowIndex].Cells["Путь"].Value.ToString()];

            textBoxAuthors.Text = additionalInfo.Authors;
            textBoxKeywords.Text = additionalInfo.Keywords;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            CheckedItems(true);
        }

        private void buttonUnselect_Click(object sender, EventArgs e)
        {
            CheckedItems(false);
        }
    }
}