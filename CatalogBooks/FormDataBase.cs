using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatalogBooks
{
    public partial class FormDataBase : Form
    {
        private BaseContext db;
        private string TableName { get; set; }

        public FormDataBase(string tableName)
        {
            InitializeComponent();
            db = new BaseContext();
            TableName = tableName;
        }

        private void SettingsTableAuthor()
        {            
            buttonDel.Visible = false;
            dataGridViewMain.DataSource = db.Authors.Local.ToBindingList();
            db.Authors.Load();
            dataGridViewMain.Columns["Books"].Visible = false;
            dataGridViewMain.Columns["AuthorId"].ReadOnly = true;
        }

        private void SettingsTableBook()
        {
            db.Books.Load();
            dataGridViewMain.DataSource = db.Books.Local.ToBindingList();
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
            db.SaveChanges();
            Close();
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewMain.SelectedRows)
                dataGridViewMain.Rows.Remove(row);
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {

        }
    }
}
