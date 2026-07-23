
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    partial class frmModSelector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private Button btnCancel;
        private Button btnOK;

        private void InitializeComponent()
        {
            btnCancel = new Button();
            btnOK = new Button();
            tabControl1 = new TabControl();
            tbMod = new TabPage();
            listMods = new ListView();
            colID = new ColumnHeader();
            colName = new ColumnHeader();
            colAuthor = new ColumnHeader();
            tbGame = new TabPage();
            chkEnableDebug = new CheckBox();
            label1 = new Label();
            comboBox1 = new ComboBox();
            tabControl1.SuspendLayout();
            tbMod.SuspendLayout();
            tbGame.SuspendLayout();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(511, 364);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(108, 41);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Enabled = false;
            btnOK.Location = new System.Drawing.Point(397, 364);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(108, 41);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tbMod);
            tabControl1.Controls.Add(tbGame);
            tabControl1.Location = new System.Drawing.Point(2, 1);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(627, 357);
            tabControl1.TabIndex = 3;
            // 
            // tbMod
            // 
            tbMod.Controls.Add(listMods);
            tbMod.Location = new System.Drawing.Point(4, 29);
            tbMod.Name = "tbMod";
            tbMod.Padding = new Padding(3);
            tbMod.Size = new System.Drawing.Size(619, 324);
            tbMod.TabIndex = 0;
            tbMod.Text = "Mod";
            tbMod.UseVisualStyleBackColor = true;
            // 
            // listMods
            // 
            listMods.Columns.AddRange(new ColumnHeader[] { colID, colName, colAuthor });
            listMods.Dock = DockStyle.Fill;
            listMods.FullRowSelect = true;
            listMods.GridLines = true;
            listMods.Location = new System.Drawing.Point(3, 3);
            listMods.MultiSelect = false;
            listMods.Name = "listMods";
            listMods.Size = new System.Drawing.Size(613, 318);
            listMods.TabIndex = 1;
            listMods.UseCompatibleStateImageBehavior = false;
            listMods.View = View.Details;
            listMods.SelectedIndexChanged += listMods_SelectedIndexChanged;
            // 
            // colID
            // 
            colID.Text = "ID";
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 260;
            // 
            // colAuthor
            // 
            colAuthor.Text = "Author";
            colAuthor.Width = 260;
            // 
            // tbGame
            // 
            tbGame.Controls.Add(chkEnableDebug);
            tbGame.Controls.Add(label1);
            tbGame.Controls.Add(comboBox1);
            tbGame.Location = new System.Drawing.Point(4, 29);
            tbGame.Name = "tbGame";
            tbGame.Padding = new Padding(3);
            tbGame.Size = new System.Drawing.Size(619, 324);
            tbGame.TabIndex = 1;
            tbGame.Text = "Game";
            tbGame.UseVisualStyleBackColor = true;
            // 
            // chkEnableDebug
            // 
            chkEnableDebug.AutoSize = true;
            chkEnableDebug.Location = new System.Drawing.Point(99, 69);
            chkEnableDebug.Name = "chkEnableDebug";
            chkEnableDebug.Size = new System.Drawing.Size(180, 24);
            chkEnableDebug.TabIndex = 2;
            chkEnableDebug.Text = "Enable Debug Mode";
            chkEnableDebug.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 17);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(87, 20);
            label1.TabIndex = 1;
            label1.Text = "languages:";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new System.Drawing.Point(99, 17);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(488, 28);
            comboBox1.TabIndex = 0;
            // 
            // frmModSelector
            // 
            ClientSize = new System.Drawing.Size(631, 417);
            Controls.Add(tabControl1);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmModSelector";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Engine Setting";
            Load += frmModSelector_Load;
            tabControl1.ResumeLayout(false);
            tbMod.ResumeLayout(false);
            tbGame.ResumeLayout(false);
            tbGame.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private TabControl tabControl1;
		private TabPage tbMod;
		private ListView listMods;
		private ColumnHeader colID;
		private ColumnHeader colName;
		private ColumnHeader colAuthor;
		private TabPage tbGame;
		private Label label1;
		private ComboBox comboBox1;
		private CheckBox chkEnableDebug;
	}
}