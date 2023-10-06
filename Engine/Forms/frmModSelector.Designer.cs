
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
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tbMod = new System.Windows.Forms.TabPage();
			this.listMods = new System.Windows.Forms.ListView();
			this.colID = new System.Windows.Forms.ColumnHeader();
			this.colName = new System.Windows.Forms.ColumnHeader();
			this.colAuthor = new System.Windows.Forms.ColumnHeader();
			this.tbGame = new System.Windows.Forms.TabPage();
			this.chkEnableDebug = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.tabControl1.SuspendLayout();
			this.tbMod.SuspendLayout();
			this.tbGame.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(511, 364);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(108, 41);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOK
			// 
			this.btnOK.Enabled = false;
			this.btnOK.Location = new System.Drawing.Point(397, 364);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(108, 41);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tbMod);
			this.tabControl1.Controls.Add(this.tbGame);
			this.tabControl1.Location = new System.Drawing.Point(2, 1);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(627, 357);
			this.tabControl1.TabIndex = 3;
			// 
			// tbMod
			// 
			this.tbMod.Controls.Add(this.listMods);
			this.tbMod.Location = new System.Drawing.Point(4, 29);
			this.tbMod.Name = "tbMod";
			this.tbMod.Padding = new System.Windows.Forms.Padding(3);
			this.tbMod.Size = new System.Drawing.Size(619, 324);
			this.tbMod.TabIndex = 0;
			this.tbMod.Text = "Mod";
			this.tbMod.UseVisualStyleBackColor = true;
			// 
			// listMods
			// 
			this.listMods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colID,
            this.colName,
            this.colAuthor});
			this.listMods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listMods.FullRowSelect = true;
			this.listMods.GridLines = true;
			this.listMods.Location = new System.Drawing.Point(3, 3);
			this.listMods.MultiSelect = false;
			this.listMods.Name = "listMods";
			this.listMods.Size = new System.Drawing.Size(613, 318);
			this.listMods.TabIndex = 1;
			this.listMods.UseCompatibleStateImageBehavior = false;
			this.listMods.View = System.Windows.Forms.View.Details;
			// 
			// colID
			// 
			this.colID.Text = "ID";
			// 
			// colName
			// 
			this.colName.Text = "Name";
			this.colName.Width = 260;
			// 
			// colAuthor
			// 
			this.colAuthor.Text = "Author";
			this.colAuthor.Width = 260;
			// 
			// tbGame
			// 
			this.tbGame.Controls.Add(this.chkEnableDebug);
			this.tbGame.Controls.Add(this.label1);
			this.tbGame.Controls.Add(this.comboBox1);
			this.tbGame.Location = new System.Drawing.Point(4, 29);
			this.tbGame.Name = "tbGame";
			this.tbGame.Padding = new System.Windows.Forms.Padding(3);
			this.tbGame.Size = new System.Drawing.Size(619, 324);
			this.tbGame.TabIndex = 1;
			this.tbGame.Text = "Game";
			this.tbGame.UseVisualStyleBackColor = true;
			// 
			// chkEnableDebug
			// 
			this.chkEnableDebug.AutoSize = true;
			this.chkEnableDebug.Location = new System.Drawing.Point(99, 69);
			this.chkEnableDebug.Name = "chkEnableDebug";
			this.chkEnableDebug.Size = new System.Drawing.Size(180, 24);
			this.chkEnableDebug.TabIndex = 2;
			this.chkEnableDebug.Text = "Enable Debug Mode";
			this.chkEnableDebug.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(87, 20);
			this.label1.TabIndex = 1;
			this.label1.Text = "languages:";
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(99, 17);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(488, 28);
			this.comboBox1.TabIndex = 0;
			// 
			// frmModSelector
			// 
			this.ClientSize = new System.Drawing.Size(631, 417);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmModSelector";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Engine Setting";
			this.Load += new System.EventHandler(this.frmModSelector_Load);
			this.tabControl1.ResumeLayout(false);
			this.tbMod.ResumeLayout(false);
			this.tbGame.ResumeLayout(false);
			this.tbGame.PerformLayout();
			this.ResumeLayout(false);

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