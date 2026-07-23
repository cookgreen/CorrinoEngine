namespace CorrinoEngine.Forms
{
    partial class frmAssetBrowser
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAssetBrowser));
            splitMain = new System.Windows.Forms.SplitContainer();
            leftPanel = new System.Windows.Forms.TableLayoutPanel();
            txtSearch = new System.Windows.Forms.TextBox();
            assetTree = new System.Windows.Forms.TreeView();
            bottomButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnExtract = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            rightPanel = new System.Windows.Forms.TableLayoutPanel();
            lblSelectedPath = new System.Windows.Forms.Label();
            previewTabs = new System.Windows.Forms.TabControl();
            tabPreview = new System.Windows.Forms.TabPage();
            previewLayout = new System.Windows.Forms.TableLayoutPanel();
            picturePreview = new System.Windows.Forms.PictureBox();
            modelPreviewHost = new System.Windows.Forms.Panel();
            txtPreviewSummary = new System.Windows.Forms.TextBox();
            tabDetails = new System.Windows.Forms.TabPage();
            txtDetails = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            leftPanel.SuspendLayout();
            bottomButtons.SuspendLayout();
            rightPanel.SuspendLayout();
            previewTabs.SuspendLayout();
            tabPreview.SuspendLayout();
            previewLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            tabDetails.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(leftPanel);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(rightPanel);
            splitMain.Size = new System.Drawing.Size(1280, 760);
            splitMain.SplitterDistance = 420;
            splitMain.TabIndex = 0;
            // 
            // leftPanel
            // 
            leftPanel.ColumnCount = 1;
            leftPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leftPanel.Controls.Add(txtSearch, 0, 0);
            leftPanel.Controls.Add(assetTree, 0, 1);
            leftPanel.Controls.Add(bottomButtons, 0, 2);
            leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            leftPanel.Location = new System.Drawing.Point(0, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.RowCount = 3;
            leftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            leftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            leftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            leftPanel.Size = new System.Drawing.Size(420, 760);
            leftPanel.TabIndex = 0;
            // 
            // txtSearch
            // 
            txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSearch.Location = new System.Drawing.Point(8, 8);
            txtSearch.Margin = new System.Windows.Forms.Padding(8, 8, 8, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search assets...";
            txtSearch.Size = new System.Drawing.Size(404, 27);
            txtSearch.TabIndex = 0;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // assetTree
            // 
            assetTree.Dock = System.Windows.Forms.DockStyle.Fill;
            assetTree.HideSelection = false;
            assetTree.Location = new System.Drawing.Point(8, 40);
            assetTree.Margin = new System.Windows.Forms.Padding(8, 4, 8, 4);
            assetTree.Name = "assetTree";
            assetTree.Size = new System.Drawing.Size(404, 668);
            assetTree.TabIndex = 1;
            assetTree.AfterSelect += assetTree_AfterSelect;
            // 
            // bottomButtons
            // 
            bottomButtons.Controls.Add(btnExtract);
            bottomButtons.Controls.Add(btnClose);
            bottomButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            bottomButtons.Location = new System.Drawing.Point(3, 715);
            bottomButtons.Name = "bottomButtons";
            bottomButtons.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            bottomButtons.Size = new System.Drawing.Size(414, 42);
            bottomButtons.TabIndex = 2;
            // 
            // btnExtract
            // 
            btnExtract.Enabled = false;
            btnExtract.Location = new System.Drawing.Point(300, 9);
            btnExtract.Name = "btnExtract";
            btnExtract.Size = new System.Drawing.Size(95, 27);
            btnExtract.TabIndex = 0;
            btnExtract.Text = "Extract";
            btnExtract.UseVisualStyleBackColor = true;
            btnExtract.Click += btnExtract_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(199, 9);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(95, 27);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // rightPanel
            // 
            rightPanel.ColumnCount = 1;
            rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rightPanel.Controls.Add(lblSelectedPath, 0, 0);
            rightPanel.Controls.Add(previewTabs, 0, 1);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.RowCount = 2;
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rightPanel.Size = new System.Drawing.Size(856, 760);
            rightPanel.TabIndex = 0;
            // 
            // lblSelectedPath
            // 
            lblSelectedPath.AutoEllipsis = true;
            lblSelectedPath.Dock = System.Windows.Forms.DockStyle.Fill;
            lblSelectedPath.Location = new System.Drawing.Point(12, 8);
            lblSelectedPath.Margin = new System.Windows.Forms.Padding(12, 8, 12, 0);
            lblSelectedPath.Name = "lblSelectedPath";
            lblSelectedPath.Size = new System.Drawing.Size(832, 32);
            lblSelectedPath.TabIndex = 0;
            lblSelectedPath.Text = "Select an asset from the left.";
            lblSelectedPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // previewTabs
            // 
            previewTabs.Controls.Add(tabPreview);
            previewTabs.Controls.Add(tabDetails);
            previewTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            previewTabs.Location = new System.Drawing.Point(12, 52);
            previewTabs.Margin = new System.Windows.Forms.Padding(12);
            previewTabs.Name = "previewTabs";
            previewTabs.SelectedIndex = 0;
            previewTabs.Size = new System.Drawing.Size(832, 696);
            previewTabs.TabIndex = 1;
            // 
            // tabPreview
            // 
            tabPreview.Controls.Add(previewLayout);
            tabPreview.Location = new System.Drawing.Point(4, 29);
            tabPreview.Name = "tabPreview";
            tabPreview.Padding = new System.Windows.Forms.Padding(3);
            tabPreview.Size = new System.Drawing.Size(824, 663);
            tabPreview.TabIndex = 0;
            tabPreview.Text = "Preview";
            tabPreview.UseVisualStyleBackColor = true;
            // 
            // previewLayout
            // 
            previewLayout.ColumnCount = 1;
            previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            previewLayout.Controls.Add(picturePreview, 0, 0);
            previewLayout.Controls.Add(modelPreviewHost, 0, 0);
            previewLayout.Controls.Add(txtPreviewSummary, 0, 1);
            previewLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            previewLayout.Location = new System.Drawing.Point(3, 3);
            previewLayout.Name = "previewLayout";
            previewLayout.RowCount = 2;
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            previewLayout.Size = new System.Drawing.Size(818, 657);
            previewLayout.TabIndex = 0;
            // 
            // picturePreview
            // 
            picturePreview.BackColor = System.Drawing.Color.FromArgb(24, 24, 24);
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.Location = new System.Drawing.Point(12, 426);
            picturePreview.Margin = new System.Windows.Forms.Padding(12);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new System.Drawing.Size(794, 198);
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabIndex = 0;
            picturePreview.TabStop = false;
            // 
            // modelPreviewHost
            // 
            modelPreviewHost.BackColor = System.Drawing.Color.Black;
            modelPreviewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            modelPreviewHost.Location = new System.Drawing.Point(12, 12);
            modelPreviewHost.Margin = new System.Windows.Forms.Padding(12);
            modelPreviewHost.Name = "modelPreviewHost";
            modelPreviewHost.Size = new System.Drawing.Size(794, 390);
            modelPreviewHost.TabIndex = 2;
            // 
            // txtPreviewSummary
            // 
            txtPreviewSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPreviewSummary.Location = new System.Drawing.Point(12, 648);
            txtPreviewSummary.Margin = new System.Windows.Forms.Padding(12);
            txtPreviewSummary.Multiline = true;
            txtPreviewSummary.Name = "txtPreviewSummary";
            txtPreviewSummary.ReadOnly = true;
            txtPreviewSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtPreviewSummary.Size = new System.Drawing.Size(794, 1);
            txtPreviewSummary.TabIndex = 1;
            // 
            // tabDetails
            // 
            tabDetails.Controls.Add(txtDetails);
            tabDetails.Location = new System.Drawing.Point(4, 29);
            tabDetails.Name = "tabDetails";
            tabDetails.Padding = new System.Windows.Forms.Padding(3);
            tabDetails.Size = new System.Drawing.Size(824, 663);
            tabDetails.TabIndex = 1;
            tabDetails.Text = "Details";
            tabDetails.UseVisualStyleBackColor = true;
            // 
            // txtDetails
            // 
            txtDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDetails.Location = new System.Drawing.Point(3, 3);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtDetails.Size = new System.Drawing.Size(818, 657);
            txtDetails.TabIndex = 0;
            txtDetails.WordWrap = false;
            // 
            // frmAssetBrowser
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 760);
            Controls.Add(splitMain);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(1080, 720);
            Name = "frmAssetBrowser";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Asset Browser";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            bottomButtons.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            previewTabs.ResumeLayout(false);
            tabPreview.ResumeLayout(false);
            previewLayout.ResumeLayout(false);
            previewLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            tabDetails.ResumeLayout(false);
            tabDetails.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TableLayoutPanel leftPanel;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.TreeView assetTree;
        private System.Windows.Forms.FlowLayoutPanel bottomButtons;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel rightPanel;
        private System.Windows.Forms.Label lblSelectedPath;
        private System.Windows.Forms.TabControl previewTabs;
        private System.Windows.Forms.TabPage tabPreview;
        private System.Windows.Forms.TableLayoutPanel previewLayout;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.Panel modelPreviewHost;
        private System.Windows.Forms.TextBox txtPreviewSummary;
        private System.Windows.Forms.TabPage tabDetails;
        private System.Windows.Forms.TextBox txtDetails;
    }
}
