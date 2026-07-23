using CorrinoEngine.Assets;
using LibEmperor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    public partial class frmAssetBrowser : Form
    {
        private readonly AssetManager assetManager;
        private readonly List<string> allFiles;
        private Bitmap currentPreviewBitmap;
        private string selectedAssetPath;

        public frmAssetBrowser(AssetManager assetManager)
        {
            this.assetManager = assetManager;
            InitializeComponent();

            allFiles = assetManager.GetAllFiles()
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            PopulateTree(allFiles);
        }

        private void PopulateTree(IEnumerable<string> files)
        {
            assetTree.BeginUpdate();
            assetTree.Nodes.Clear();

            foreach (string file in files)
            {
                string[] parts = file.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    continue;
                }

                TreeNodeCollection currentNodes = assetTree.Nodes;
                TreeNode currentNode = null;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    TreeNode next = currentNodes
                        .Cast<TreeNode>()
                        .FirstOrDefault(node => string.Equals(node.Text, part, StringComparison.OrdinalIgnoreCase));
                    if (next == null)
                    {
                        next = new TreeNode(part);
                        currentNodes.Add(next);
                    }

                    currentNode = next;
                    currentNodes = next.Nodes;
                }

                if (currentNode != null)
                {
                    currentNode.Tag = file;
                }
            }

            assetTree.Sort();
            assetTree.ExpandAll();
            assetTree.EndUpdate();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text?.Trim() ?? string.Empty;
            IEnumerable<string> filtered = string.IsNullOrWhiteSpace(keyword)
                ? allFiles
                : allFiles.Where(path => path.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            PopulateTree(filtered);
        }

        private void assetTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string assetPath = e.Node?.Tag as string;
            btnExtract.Enabled = !string.IsNullOrWhiteSpace(assetPath);

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                ClearPreview("Select a file node to preview.");
                return;
            }

            selectedAssetPath = assetPath;
            lblSelectedPath.Text = assetPath;

            try
            {
                PreviewAsset(assetPath);
            }
            catch (Exception ex)
            {
                ClearPreview("Preview failed.");
                txtPreviewSummary.Text = $"Preview error:{Environment.NewLine}{ex.Message}";
                txtDetails.Text = ex.ToString();
            }
        }

        private void PreviewAsset(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            if (extension == ".tga")
            {
                PreviewTexture(assetPath);
                return;
            }

            if (extension == ".xbf")
            {
                PreviewXbf(assetPath);
                return;
            }

            PreviewGeneric(assetPath);
        }

        private void PreviewTexture(string assetPath)
        {
            using Stream stream = assetManager.Read(assetPath);
            if (stream == null)
            {
                throw new FileNotFoundException("Asset stream was null.", assetPath);
            }

            var tga = new Tga(stream);
            SetPreviewBitmap(CreateBitmapFromRgba(tga.Width, tga.Height, tga.Pixels));

            txtPreviewSummary.Text =
                $"Type: Texture (.tga){Environment.NewLine}" +
                $"Path: {assetPath}{Environment.NewLine}" +
                $"Size: {tga.Width} x {tga.Height}{Environment.NewLine}" +
                $"Pixels: {tga.Pixels.Length}";

            txtDetails.Text = txtPreviewSummary.Text;
        }

        private void PreviewXbf(string assetPath)
        {
            using Stream stream = assetManager.Read(assetPath);
            if (stream == null)
            {
                throw new FileNotFoundException("Asset stream was null.", assetPath);
            }

            var xbf = new Xbf(stream);
            SetPreviewBitmap(null);

            int objectCount = CountObjects(xbf.Objects);
            int vertexCount = SumVertices(xbf.Objects);
            int triangleCount = SumTriangles(xbf.Objects);

            txtPreviewSummary.Text =
                $"Type: Mesh (.xbf){Environment.NewLine}" +
                $"Path: {assetPath}{Environment.NewLine}" +
                $"Root Objects: {xbf.Objects.Count}{Environment.NewLine}" +
                $"Total Objects: {objectCount}{Environment.NewLine}" +
                $"Vertices: {vertexCount}{Environment.NewLine}" +
                $"Triangles: {triangleCount}{Environment.NewLine}" +
                $"Textures: {xbf.Textures.Length}";

            var builder = new StringBuilder();
            builder.AppendLine($"Type: Mesh (.xbf)");
            builder.AppendLine($"Path: {assetPath}");
            builder.AppendLine($"Root Objects: {xbf.Objects.Count}");
            builder.AppendLine($"Total Objects: {objectCount}");
            builder.AppendLine($"Vertices: {vertexCount}");
            builder.AppendLine($"Triangles: {triangleCount}");
            builder.AppendLine($"Textures: {xbf.Textures.Length}");
            builder.AppendLine();

            if (xbf.Textures.Length > 0)
            {
                builder.AppendLine("Texture References:");
                foreach (string texture in xbf.Textures)
                {
                    builder.AppendLine($"  - {texture}");
                }
                builder.AppendLine();
            }

            builder.AppendLine("Object Hierarchy:");
            foreach (XbfObject obj in xbf.Objects)
            {
                AppendObject(builder, obj, 0);
            }

            txtDetails.Text = builder.ToString();
        }

        private void PreviewGeneric(string assetPath)
        {
            using Stream stream = assetManager.Read(assetPath);
            if (stream == null)
            {
                throw new FileNotFoundException("Asset stream was null.", assetPath);
            }

            SetPreviewBitmap(null);
            txtPreviewSummary.Text =
                $"Type: {Path.GetExtension(assetPath)}{Environment.NewLine}" +
                $"Path: {assetPath}{Environment.NewLine}" +
                $"Length: {stream.Length} bytes";

            txtDetails.Text = txtPreviewSummary.Text;
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedAssetPath))
            {
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = Path.GetFileName(selectedAssetPath);
            dialog.Filter = "All Files|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using Stream input = assetManager.Read(selectedAssetPath);
            using FileStream output = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            input.CopyTo(output);
            MessageBox.Show("Asset extracted.", "Asset Browser", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ClearPreview(string message)
        {
            lblSelectedPath.Text = message;
            SetPreviewBitmap(null);
            txtPreviewSummary.Text = message;
            txtDetails.Text = message;
            selectedAssetPath = null;
            btnExtract.Enabled = false;
        }

        private void SetPreviewBitmap(Bitmap bitmap)
        {
            currentPreviewBitmap?.Dispose();
            currentPreviewBitmap = bitmap;
            picturePreview.Image = currentPreviewBitmap;
        }

        private static Bitmap CreateBitmapFromRgba(int width, int height, byte[] pixels)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] bgra = new byte[pixels.Length];
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    bgra[i] = pixels[i + 2];
                    bgra[i + 1] = pixels[i + 1];
                    bgra[i + 2] = pixels[i];
                    bgra[i + 3] = pixels[i + 3];
                }

                System.Runtime.InteropServices.Marshal.Copy(bgra, 0, data.Scan0, bgra.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private static int CountObjects(IEnumerable<XbfObject> objects)
        {
            int count = 0;
            foreach (XbfObject obj in objects)
            {
                count += 1;
                count += CountObjects(obj.Children);
            }

            return count;
        }

        private static int SumVertices(IEnumerable<XbfObject> objects)
        {
            int count = 0;
            foreach (XbfObject obj in objects)
            {
                count += obj.Vertices.Length;
                count += SumVertices(obj.Children);
            }

            return count;
        }

        private static int SumTriangles(IEnumerable<XbfObject> objects)
        {
            int count = 0;
            foreach (XbfObject obj in objects)
            {
                count += obj.Triangles.Length;
                count += SumTriangles(obj.Children);
            }

            return count;
        }

        private static void AppendObject(StringBuilder builder, XbfObject obj, int depth)
        {
            string indent = new string(' ', depth * 2);
            string displayName = string.IsNullOrWhiteSpace(obj.Name) ? "<unnamed>" : obj.Name;
            builder.AppendLine($"{indent}- {displayName} | vertices={obj.Vertices.Length}, triangles={obj.Triangles.Length}, children={obj.Children.Length}");
            foreach (XbfObject child in obj.Children)
            {
                AppendObject(builder, child, depth + 1);
            }
        }
    }
}
