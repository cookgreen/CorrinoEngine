using CorrinoEngine.Assets;
using CorrinoEngine.Maps;
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
            using Stream mapStream = assetManager.Read(xbfPath);
            if (mapStream == null)
            {
                MessageBox.Show("Cannot read selected map XBF.", "Map Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MapXbf mapXbf = MapXbf.Load(mapStream);
            bool hasGroundColor = assetManager.Read(mapDir + "/test.CPT") != null;
            bool hasGroundPalette = assetManager.Read(mapDir + "/test.CPF") != null;
            bool hasGroundLight = assetManager.Read(mapDir + "/texture.dat") != null;
            bool hasGroundLit = assetManager.Read(mapDir + "/test.lit") != null;
            int width = mapXbf.MapSize.X > 0 ? mapXbf.MapSize.X : 128;
            int height = mapXbf.MapSize.Y > 0 ? mapXbf.MapSize.Y : 128;

            var builder = new StringBuilder();
            builder.AppendLine("Map:");
            builder.AppendLine($"\tName: {mapName}");
            builder.AppendLine("\tAuthor: Imported from original map");
            builder.AppendLine($"\tWidth: {width}");
            builder.AppendLine($"\tHeight: {height}");
            builder.AppendLine("\tTileSize: 32");
            builder.AppendLine("\tTileUvScale: 1");
            builder.AppendLine();
            builder.AppendLine("Metadata:");
            builder.AppendLine($"\tOriginalMapDir: {mapDir}");
            builder.AppendLine($"\tOriginalMapXbf: {xbfPath}");
            builder.AppendLine($"\tGroundColor: {(hasGroundColor ? mapDir + "/test.CPT" : string.Empty)}");
            builder.AppendLine($"\tGroundPalette: {(hasGroundPalette ? mapDir + "/test.CPF" : string.Empty)}");
            builder.AppendLine($"\tGroundLight: {(hasGroundLight ? mapDir + "/texture.dat" : string.Empty)}");
            builder.AppendLine($"\tGroundLit: {(hasGroundLit ? mapDir + "/test.lit" : string.Empty)}");
            builder.AppendLine("\tMapScale: 0.0625");

            File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
            MessageBox.Show($"Converted to {outputPath}", "Map Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
