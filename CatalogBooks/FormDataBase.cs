using System;
using System.Data.Entity;
using System.Windows.Forms;

namespace CatalogBooks
{
    public partial class FormDataBase : Form
    {
        private readonly BaseContext _db;
        private string TableName { get; }

        public FormDataBase(string tableName)
        {
            InitializeComponent();
            _db = new BaseContext();
            TableName = tableName;
        }

        private void SettingsTableAuthor()
        {            
            buttonDel.Visible = false;
            dataGridViewMain.DataSource = _db.Authors.Local.ToBindingList();
            _db.Authors.Load();
            dataGridViewMain.Columns["Books"].Visible = false;
            dataGridViewMain.Columns["AuthorId"].ReadOnly = true;
        }

        private void SettingsTableBook()
        {
            _db.Books.Load();
            dataGridViewMain.DataSource = _db.Books.Local.ToBindingList();
            dataGridViewMain.Columns["Authors"].Visible = false;
            dataGridViewMain.Columns["BookId"].ReadOnly = true;
            dataGridViewMain.Columns["MD5"].ReadOnly = true;
            dataGridViewMain.Columns["Path"].ReadOnly = true;
        }

        private void FormDataBase_Load(object sender, EventArgs e)
        {                     
            if (TableName == "Книги")
                SettingsTableBook();
            else if (TableName == "Авторы")
                SettingsTableAuthor();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            _db.SaveChanges();
            Close();
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (dataGridViewMain.SelectedRows.Count == 0)
                MessageBox.Show("Для удаления строк выделите их полностью.", "Подсказка");

            foreach (DataGridViewRow row in dataGridViewMain.SelectedRows)
                dataGridViewMain.Rows.Remove(row);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {

        }
    }
}
