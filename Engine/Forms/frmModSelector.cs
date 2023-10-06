using CorrinoEngine.Mods;
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
    public partial class frmModSelector : Form
    {
        private Argument argument;
        public Argument Argument
        {
            get { return argument; }
        }

        public frmModSelector()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listMods.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Please select a valid mod!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var item = listMods.SelectedItems[0];
            var id = item.SubItems[0].Text;

            string[] strArr = new string[]
            {
                "Mod=" + id
            };
            argument = new Argument(strArr);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmModSelector_Load(object sender, EventArgs e)
        {
            listMods.Items.Clear();
            foreach(var mod in ModManager.Instance.Mods)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = mod.Key;
                lvi.SubItems.Add(mod.Value.Manifest.MetaData.Name);
                lvi.SubItems.Add(mod.Value.Manifest.MetaData.Author);
                listMods.Items.Add(lvi);
            }
        }

        private void listMods_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listMods.SelectedItems.Count == 0)
            {
                btnOK.Enabled = false;
            }
            else
            {
                btnOK.Enabled = true;
            }
        }
    }
}
