using CorrinoEngine.Assets;
using CorrinoEngine.Cameras;
using CorrinoEngine.Graphics.Mesh;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Windows.Forms;

namespace CorrinoEngine.Forms
{
    public class AssetPreviewControl : UserControl
    {
        private readonly GLControl glControl;
        private readonly Timer renderTimer;
        private readonly AssetManager assetManager;
        private readonly PreviewAssetHolder previewHolder;
        private PerspectiveCamera camera;
        private MeshInstance previewMeshInstance;
        private string currentAssetPath;
        private bool isInitialized;
        private bool isDragging;
        private System.Drawing.Point lastMousePosition;
        private float yaw = -0.85f;
        private float pitch = -0.35f;
        private float distance = 220f;

        public AssetPreviewControl(AssetManager assetManager)
        {
            this.assetManager = assetManager;
            previewHolder = new PreviewAssetHolder();
            Dock = DockStyle.Fill;
            BackColor = System.Drawing.Color.Black;

            glControl = new GLControl(new GLControlSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(3, 3, 0, 0),
                Profile = ContextProfile.Core
            })
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };

            Controls.Add(glControl);

            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseUp += GlControl_MouseUp;
            glControl.MouseWheel += GlControl_MouseWheel;

            renderTimer = new Timer { Interval = 16 };
            renderTimer.Tick += RenderTimer_Tick;
        }

        public void PreviewXbf(string assetPath)
        {
            currentAssetPath = assetPath;
            if (!isInitialized)
            {
                return;
            }

            LoadPreviewMesh();
        }

        public void ClearPreview()
        {
            currentAssetPath = null;
            UnloadPreviewMesh();
            if (isInitialized)
            {
                glControl.Invalidate();
            }
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            camera = new PerspectiveCamera
            {
                Size = new Vector2(Math.Max(glControl.Width, 1), Math.Max(glControl.Height, 1)),
                Fov = 50f
            };
            UpdateCamera();

            isInitialized = true;
            renderTimer.Start();

            if (!string.IsNullOrWhiteSpace(currentAssetPath))
            {
                LoadPreviewMesh();
            }
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!isInitialized)
            {
                return;
            }

            glControl.MakeCurrent();
            GL.Viewport(0, 0, Math.Max(glControl.Width, 1), Math.Max(glControl.Height, 1));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (previewMeshInstance != null)
            {
                previewMeshInstance.Draw(camera);
            }

            glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                return;
            }

            glControl.MakeCurrent();
            GL.Viewport(0, 0, Math.Max(glControl.Width, 1), Math.Max(glControl.Height, 1));
            camera.Size = new Vector2(Math.Max(glControl.Width, 1), Math.Max(glControl.Height, 1));
            UpdateCamera();
            glControl.Invalidate();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (!isInitialized)
            {
                return;
            }

            previewMeshInstance?.Update(1f / 60f);
            glControl.Invalidate();
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            isDragging = true;
            lastMousePosition = e.Location;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                return;
            }

            int deltaX = e.X - lastMousePosition.X;
            int deltaY = e.Y - lastMousePosition.Y;
            lastMousePosition = e.Location;

            yaw += deltaX * 0.01f;
            pitch = Math.Clamp(pitch + deltaY * 0.01f, -1.2f, 1.2f);
            UpdateCamera();
            glControl.Invalidate();
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            distance = Math.Clamp(distance - e.Delta * 0.08f, 40f, 1200f);
            UpdateCamera();
            glControl.Invalidate();
        }

        private void UpdateCamera()
        {
            if (camera == null)
            {
                return;
            }

            Vector3 target = Vector3.Zero;
            Vector3 offset = new Vector3(
                MathF.Cos(pitch) * MathF.Sin(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Cos(yaw)) * distance;

            camera.Position = target + offset;
            camera.Direction = (target - camera.Position).Normalized();
            camera.Update();
        }

        private void LoadPreviewMesh()
        {
            glControl.MakeCurrent();
            UnloadPreviewMesh();

            if (string.IsNullOrWhiteSpace(currentAssetPath))
            {
                return;
            }

            var mesh = assetManager.Load<XbfMesh>(previewHolder, currentAssetPath);
            previewMeshInstance = new MeshInstance(mesh)
            {
                Speed = 18f
            };
            previewMeshInstance.Position = Vector3.Zero;

            yaw = -0.85f;
            pitch = -0.35f;
            distance = EstimateDistance(mesh);
            UpdateCamera();
        }

        private void UnloadPreviewMesh()
        {
            previewMeshInstance = null;
            assetManager.Unload(previewHolder);
        }

        private static float EstimateDistance(Mesh mesh)
        {
            int triangleCount = CountTriangles(mesh);
            if (triangleCount < 64)
            {
                return 120f;
            }

            if (triangleCount < 1024)
            {
                return 220f;
            }

            return 320f;
        }

        private static int CountTriangles(Mesh mesh)
        {
            int count = 0;
            if (mesh.Children != null)
            {
                foreach (Mesh child in mesh.Children)
                {
                    count += CountTriangles(child);
                }
            }

            return Math.Max(1, count + 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                renderTimer?.Stop();
                UnloadPreviewMesh();
                renderTimer?.Dispose();
                glControl?.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class PreviewAssetHolder
        {
        }
    }
}
