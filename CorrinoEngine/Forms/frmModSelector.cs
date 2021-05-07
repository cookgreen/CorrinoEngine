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
        public frmModSelector()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                return;
            }

            var item = listView1.SelectedItems[0];
            var id = item.SubItems[0].Text;

            string[] strArr = new string[]
            {
                "Mod="+id
            };
            Argument argument = new Argument(strArr);

            Hide();
            DialogResult = DialogResult.OK;

            GameApp app = new GameApp(argument);
            app.Run();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmModSelector_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            foreach(var mod in ModManager.Instance.Mods)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = mod.Key;
                lvi.SubItems.Add(mod.Value.Manifest.MetaData.Name);
                lvi.SubItems.Add(mod.Value.Manifest.MetaData.Author);
                listView1.Items.Add(lvi);
            }
        }
    }
}
