using CorrinoEngine.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    public class frmMapImport : Form
    {
        private readonly AssetManager assetManager;
        private readonly TextBox txtMapPath;
        private readonly TextBox txtOutputName;
        private readonly Button btnConvert;
        private readonly ListBox lstMaps;

        public frmMapImport(AssetManager assetManager)
        {
            this.assetManager = assetManager;
            Text = "Import Original Map";
            Width = 720;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;

            var topPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 92,
                ColumnCount = 2
            };
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));

            txtMapPath = new TextBox { Dock = DockStyle.Top, ReadOnly = true };
            txtOutputName = new TextBox { Dock = DockStyle.Top, PlaceholderText = "output yaml name, e.g. imported-map.yaml" };
            btnConvert = new Button { Dock = DockStyle.Fill, Text = "Convert To YAML" };
            btnConvert.Click += btnConvert_Click;

            var leftTop = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            leftTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            leftTop.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
            leftTop.Controls.Add(txtMapPath, 0, 0);
            leftTop.Controls.Add(txtOutputName, 0, 1);

            topPanel.Controls.Add(leftTop, 0, 0);
            topPanel.Controls.Add(btnConvert, 1, 0);

            lstMaps = new ListBox { Dock = DockStyle.Fill };
            lstMaps.SelectedIndexChanged += lstMaps_SelectedIndexChanged;

            Controls.Add(lstMaps);
            Controls.Add(topPanel);

            LoadMaps();
        }

        private void LoadMaps()
        {
            lstMaps.Items.Clear();
            var candidates = assetManager.GetAllFiles()
                .Select(path => path.Replace('\\', '/'))
                .Where(path => path.Contains("MAPS/", StringComparison.OrdinalIgnoreCase))
                .Where(path => path.EndsWith("/test.xbf", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/debug.xbf", StringComparison.OrdinalIgnoreCase))
                .Select(path => path.Substring(0, path.LastIndexOf('/')))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string candidate in candidates)
            {
                lstMaps.Items.Add(candidate);
            }
        }

        private void lstMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMaps.SelectedItem == null)
            {
                return;
            }

            string mapPath = lstMaps.SelectedItem.ToString();
            txtMapPath.Text = mapPath;
            if (string.IsNullOrWhiteSpace(txtOutputName.Text))
            {
                string safeName = mapPath.Split('/').Last().Replace(' ', '-').Replace('#', '_');
                txtOutputName.Text = safeName + ".yaml";
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMapPath.Text) || string.IsNullOrWhiteSpace(txtOutputName.Text))
            {
                MessageBox.Show("Select a source map and output file name first.", "Map Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mapsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods", "ebfd", "Maps");
            Directory.CreateDirectory(mapsDir);
            string outputPath = Path.Combine(mapsDir, txtOutputName.Text);

            string mapDir = txtMapPath.Text.Replace('\\', '/');
            string mapName = mapDir.Split('/').Last();
            string xbfPath = assetManager.Read(mapDir + "/test.xbf") != null ? mapDir + "/test.xbf" : mapDir + "/debug.xbf";
            bool hasGroundColor = assetManager.Read(mapDir + "/test.CPT") != null;
            bool hasGroundLight = assetManager.Read(mapDir + "/texture.dat") != null;

            var builder = new StringBuilder();
            builder.AppendLine("Map:");
            builder.AppendLine($"\tName: {mapName}");
            builder.AppendLine("\tAuthor: Imported from original map");
            builder.AppendLine("\tWidth: 128");
            builder.AppendLine("\tHeight: 128");
            builder.AppendLine("\tTileSize: 64");
            builder.AppendLine($"\tTileResource: {xbfPath}");
            builder.AppendLine("\tTileUvScale: 1");
            builder.AppendLine();
            builder.AppendLine("Metadata:");
            builder.AppendLine($"\tOriginalMapDir: {mapDir}");
            builder.AppendLine($"\tGroundColor: {(hasGroundColor ? "test.CPT" : string.Empty)}");
            builder.AppendLine($"\tGroundLight: {(hasGroundLight ? "texture.dat" : string.Empty)}");

            File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
            MessageBox.Show($"Converted to {outputPath}", "Map Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
