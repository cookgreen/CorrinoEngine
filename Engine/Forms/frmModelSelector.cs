using CorrinoEngine.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    public partial class frmModelSelector : Form
    {
        private AssetManager assetManager;
        public string SelectedModel { get; set; }

        public frmModelSelector(AssetManager assetManager, string currentFile = null)
        {
            InitializeComponent();
            this.assetManager = assetManager;

            loadXbfFileList();

            if (!string.IsNullOrEmpty(currentFile))
            {
                xbfModelList.SelectedItem = currentFile;
            }

            btnSaveListAs.Enabled = true;
        }

        private void loadXbfFileList()
        {
            xbfModelList.Items.Clear();
            List<string> xbfFiles = assetManager.GetFilesByExtension("xbf").ToList();
            foreach(var xbfFile in xbfFiles)
            {
                xbfModelList.Items.Add(xbfFile);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (xbfModelList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a model!");
            }
            else
            {
                SelectedModel = xbfModelList.SelectedItem.ToString();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void xbfModelList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (xbfModelList.SelectedIndex != -1)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        private void btnSaveListAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text File|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var item in xbfModelList.Items)
                    {
                        writer.WriteLine(item.ToString());
                    }
                }
            }
        }
    }
}
