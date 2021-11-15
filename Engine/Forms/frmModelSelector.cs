using CorrinoEngine.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public frmModelSelector(AssetManager assetManager)
        {
            InitializeComponent();
            this.assetManager = assetManager;

            loadXbfFileList();
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
    }
}
