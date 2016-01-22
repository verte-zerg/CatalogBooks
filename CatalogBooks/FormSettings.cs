using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CatalogBooks
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            checkBoxScan.Checked = Properties.Settings.Default.AutoScan;
            listBoxPaths.Items.AddRange(Properties.Settings.Default.PathsScan.Split(';').Where(s => !String.IsNullOrEmpty(s)).ToArray());
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                textBoxPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string path = textBoxPath.Text.Trim();
            if (path != "" && listBoxPaths.Items.IndexOf(path) == -1 && Directory.Exists(path))
                listBoxPaths.Items.Add(path);            

            textBoxPath.Clear();            
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (listBoxPaths.SelectedItem != null)
                listBoxPaths.Items.RemoveAt(listBoxPaths.SelectedIndex);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoScan = checkBoxScan.Checked;
            string paths = listBoxPaths.Items.Cast<string>().Aggregate("", (current, s) => current + (s + ";"));
            Properties.Settings.Default.PathsScan = paths;
            Properties.Settings.Default.Save();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
