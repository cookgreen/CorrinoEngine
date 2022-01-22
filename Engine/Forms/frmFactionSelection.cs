using CorrinoEngine.Core;
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
    public partial class frmFactionSelection : Form
    {
        private List<FactionInfo> factionInfos;
        private FactionInfo selectedFactionInfo;
        
        public FactionInfo SelectedFactionInfo
        {
            get { return selectedFactionInfo; }
        }

        public frmFactionSelection(List<FactionInfo> factionInfos)
        {
            this.factionInfos = factionInfos;
            InitializeComponent();
            refreshFactionList();
        }

        private void refreshFactionList()
        {
            cmbFactionSelection.Items.Clear();

            foreach(var factionInfo in factionInfos)
            {
                cmbFactionSelection.Items.Add(factionInfo.ID);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cmbFactionSelection.SelectedIndex != -1)
            {
                selectedFactionInfo = factionInfos.Where(o => o.ID == cmbFactionSelection.SelectedItem.ToString()).First();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmbFactionSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            var result = factionInfos.Where(o => o.ID == cmbFactionSelection.SelectedItem.ToString()).FirstOrDefault();
            txtStartUnit.Text = result == null ? string.Empty : result.StartActor;
        }
    }
}
