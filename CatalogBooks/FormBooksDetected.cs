using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogBooks
{
    /// <summary>
    /// Структура для хранения дополнительной информации
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
        private readonly BaseContext _db;
        /// <summary>
        /// Объект для хранинения отображаемой информации о файлах
        /// </summary>
        private readonly DataTable _dt;
        /// <summary>
        /// Объект для сканирования папок
        /// </summary>
        private readonly CheckFiles _checkFiles;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public FormBooksDetected()
        {
            InitializeComponent();
            _db = new BaseContext();
            _db.Books.Load();
            _db.Authors.Load();
            DictAuthor = new Dictionary<string, AdditionalInfo>();
            _dt = new DataTable("Books");
            _checkFiles = new CheckFiles(".pdf|.djvu|.fb2");
            ListBooks = new List<Book>();
            LoadData();
        }

        /// <summary>
        /// Загрузка данных в DataTable
        /// </summary>
        private void LoadData()
        {           
            _dt.Columns.Add("Добавить", typeof(Boolean));
            _dt.Columns.Add("Название", typeof(String));
            _dt.Columns.Add("Год издания", typeof(Int32));
            _dt.Columns.Add("Путь", typeof(String));            

            dataGridViewMain.DataSource = _dt;

            dataGridViewMain.Columns["Путь"].ReadOnly = true;
        }

        /// <summary>
        /// Добавление элемента в таблицу
        /// </summary>
        /// <param name="book">Книга</param>
        private void AddItem(Book book)
        {
            ListBooks.Add(book);
            string name = Path.GetFileNameWithoutExtension(book.Path);
            _dt.Rows.Add(true, name, 2000, book.Path);
            DictAuthor[book.Path] = new AdditionalInfo()
            {
                Authors = "",
                Keywords = ""
            };
        }

        /// <summary>
        /// Проверка на вхождение в базу данных
        /// </summary>
        /// <param name="book"></param>
        /// <returns>Входимость</returns>
        private bool Contains(Book book)
        {
            return _db.Books.Any(item => item.MD5 == book.MD5);
        }

        /// <summary>
        /// Сохранение изменений а базе данных
        /// </summary>
        private void SaveData()
        {
            for (int i = 0; i < ListBooks.Count; i++)
            {
                ListBooks[i].Name = _dt.Rows[i].ItemArray[1].ToString();
                ListBooks[i].Year = (int) _dt.Rows[i].ItemArray[2];
            }

            for (int i = 0; i < ListBooks.Count; i++)
                if ((bool) _dt.Rows[i].ItemArray[0])
                {
                    ListBooks[i].KeyWords = DictAuthor[ListBooks[i].Path].Keywords;
                    _db.Books.Add(ListBooks[i]);

                    string authors = DictAuthor[ListBooks[i].Path].Authors;

                    if (authors == "")
                        authors = "noname noname";

                    foreach (string authorInfo in authors.Split(';'))
                    {
                        string firstName = authorInfo.Substring(authorInfo.IndexOf(' ') + 1);
                        string lastName = authorInfo.Substring(0, authorInfo.IndexOf(' '));
                        if (!_db.Authors.Any(x => x.FirstName == firstName && x.LastName == lastName))
                            _db.Authors.Add(new Author()
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Books = new List<Book>() {ListBooks[i]}
                            });
                        else
                        {
                            Author author =
                                _db.Authors.First(x => x.FirstName == firstName && x.LastName == lastName);
                            author.Books.Add(ListBooks[i]);
                        }
                    }

                    _db.SaveChanges();
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
        private async Task CheckDirectoryAsync()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string[] folders =
                Properties.Settings.Default.PathsScan.Split(';').Where(x => !String.IsNullOrEmpty(x)).ToArray();

            _checkFiles.NewBook.Subscribe
            (
                onNext: x =>
                {
                    toolStripStatusLabelInfo.Text = String.Format("Проверка файла: {0}", x.Path);
                    if (!Contains(x))
                        AddItem(x);
                },
                onCompleted: () =>
                {
                    sw.Stop();

                    if (ListBooks.Count == 0)
                        toolStripStatusLabelInfo.Text = "Сканирование не дало результатов. ";
                    else
                        toolStripStatusLabelInfo.Text = String.Format("Было обнаружено: {0} файлов. ", ListBooks.Count);

                    toolStripStatusLabelInfo.Text += String.Format("Время: {0:F2} сек.", sw.Elapsed.TotalSeconds);
                }
            );

            await _checkFiles.CheckForldersAsync(folders);                   
        } 

        private async void FormBooksDetected_Load(object sender, EventArgs e)
        {
            await CheckDirectoryAsync();
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
            string fileName = dataGridViewMain.Rows[e.RowIndex].Cells["Путь"].Value.ToString();
            AdditionalInfo additionalInfo = DictAuthor[fileName];

            buttonOpen.Tag = fileName;
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

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            string fileName = (sender as Control).Tag.ToString();

            if (File.Exists(fileName))
                Process.Start(fileName);
            else
                MessageBox.Show(String.Format("Файл '{0}'", fileName));

        }
    }
}