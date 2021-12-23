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
    public partial class frmRFHRFDFileListViewer : Form
    {
        private AssetManager assetManager;

        public frmRFHRFDFileListViewer(AssetManager assetManager)
        {
            this.assetManager = assetManager;
            InitializeComponent();

            LoadFileList();
        }

        private void LoadFileList()
        {
            fileList.Items.Clear();
            List<string> xbfFiles = assetManager.GetAllFiles().ToList();
            foreach (var xbfFile in xbfFiles)
            {
                fileList.Items.Add(xbfFile);
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text File|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    foreach (var item in fileList.Items)
                    {
                        writer.WriteLine(item.ToString());
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = fileList.SelectedItem.ToString();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                string outputFullPath = dialog.FileName;
                var ms = assetManager.Read(fileList.SelectedItem.ToString());

                using (FileStream file = new FileStream(outputFullPath, FileMode.Create, FileAccess.Write))
                {
                    byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }

                MessageBox.Show("Success!");
            }
        }

        private void fileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fileList.SelectedIndex > -1)
            {
                btnExtract.Enabled = true;
            }
            else
            {
                btnExtract.Enabled = false;
            }
        }
    }
}
