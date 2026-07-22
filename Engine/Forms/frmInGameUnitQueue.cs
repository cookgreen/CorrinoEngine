using CorrinoEngine.Core;
using CorrinoEngine.Fields;
using CorrinoEngine.Translation;
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
    public partial class frmInGameUnitQueue : Form
    {
        private static frmInGameUnitQueue instance;
        private World world;

        public static frmInGameUnitQueue Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new frmInGameUnitQueue();
                }
                return instance;
            }
        }

        public frmInGameUnitQueue()
        {
            InitializeComponent();
            bindEvents();
            Hide();
            ShowInTaskbar = false;
        }

        public void BindWorld(World world)
        {
            this.world = world;
        }

        public void UpdateData()
        {
            listView1.Items.Clear();
            listView3.Items.Clear();
            listView2.Items.Clear();

            if (world == null || world.SelectedActor == null)
            {
                Text = "Unit Queue";
                button1.Enabled = false;
                button3.Enabled = false;
                return;
            }

            Text = "Unit Queue - " + world.SelectedActor.ActorData.TypeName;

            foreach (var actorData in world.GetBuildableActors())
            {
                string displayName = GetDisplayName(actorData);
                string displayDesc = GetDescription(actorData);
                ListViewItem item = new ListViewItem(new string[] { displayName, displayDesc })
                {
                    Tag = actorData.TypeName
                };

                if (actorData.TypeName.Contains("infantry"))
                {
                    listView3.Items.Add(item);
                }
                else
                {
                    listView1.Items.Add(item);
                }
            }

            button1.Enabled = GetSelectedProductionType() != null;
            button3.Enabled = listView2.Items.Count > 0;
        }

        private void bindEvents()
        {
            button1.Click += button1_Click;
            button3.Click += button3_Click;
            listView1.SelectedIndexChanged += productionList_SelectedIndexChanged;
            listView3.SelectedIndexChanged += productionList_SelectedIndexChanged;
            FormClosing += frmInGameUnitQueue_FormClosing;
        }

        private void productionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = GetSelectedProductionType() != null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string actorTypeName = GetSelectedProductionType();
            if (world == null || string.IsNullOrWhiteSpace(actorTypeName))
            {
                return;
            }

            world.EnqueueBuild(actorTypeName);
            string displayName = actorTypeName;
            listView2.Items.Add(new ListViewItem(new string[] { displayName, "Done" }));
            button3.Enabled = listView2.Items.Count > 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView2.Items.Count > 0)
            {
                listView2.Items.RemoveAt(listView2.Items.Count - 1);
            }

            button3.Enabled = listView2.Items.Count > 0;
        }

        private void frmInGameUnitQueue_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private string GetSelectedProductionType()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                return listView1.SelectedItems[0].Tag?.ToString();
            }

            if (listView3.SelectedItems.Count > 0)
            {
                return listView3.SelectedItems[0].Tag?.ToString();
            }

            return null;
        }

        private string GetDisplayName(ActorData actorData)
        {
            object nameValue = actorData.DataField.Properties.ContainsKey("Name")
                ? actorData.DataField.Properties["Name"]
                : actorData.TypeName;

            string rawValue = nameValue?.ToString() ?? actorData.TypeName;
            if (rawValue.isTransableString())
            {
                return rawValue.ToTransableString().Translate("English");
            }

            return rawValue;
        }

        private string GetDescription(ActorData actorData)
        {
            if (!actorData.DataField.Properties.ContainsKey("Description"))
            {
                return string.Empty;
            }

            string rawValue = actorData.DataField.Properties["Description"]?.ToString() ?? string.Empty;
            if (rawValue.isTransableString())
            {
                return rawValue.ToTransableString().Translate("English");
            }

            return rawValue;
        }
    }
}
