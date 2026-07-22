using System;
using CorrinoEngine.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;
using System.Linq;

namespace CorrinoEngine.Renderer
{
    public class HudRenderer : IDisposable
    {
        private Vector2 viewportSize;
        private TextRenderer textRenderer;
        private readonly Font titleFont;
        private readonly Font bodyFont;
        private readonly Brush whiteBrush;
        private readonly Brush accentBrush;
        private readonly Brush dimBrush;
        private bool disposed;

        public HudRenderer(Vector2 viewportSize)
        {
            this.viewportSize = viewportSize;
            textRenderer = new TextRenderer((int)viewportSize.X, (int)viewportSize.Y);
            titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            bodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
            whiteBrush = Brushes.White;
            accentBrush = new SolidBrush(Color.FromArgb(255, 220, 186, 109));
            dimBrush = new SolidBrush(Color.FromArgb(255, 180, 180, 180));
        }

        public void Resize(Vector2 size)
        {
            viewportSize = size;
            textRenderer.Dispose();
            textRenderer = new TextRenderer((int)size.X, (int)size.Y);
        }

        public void Render(World world)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, viewportSize.X, viewportSize.Y, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            DrawPanels(world);
            DrawHudText(world);

            GL.Enable(EnableCap.Texture2D);
            DrawTextOverlay();
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void DrawPanels(World world)
        {
            DrawRect(12, 12, 320, 136, Color.FromArgb(170, 10, 14, 18));
            DrawRect(12, viewportSize.Y - 136, 420, 124, Color.FromArgb(185, 12, 16, 20));
            DrawRect(viewportSize.X - 300, 12, 288, 100, Color.FromArgb(160, 10, 14, 18));

            if (world.SelectedActor != null)
            {
                DrawRect(16, viewportSize.Y - 132, 412, 4, Color.FromArgb(220, 196, 160, 92));
            }
        }

        private void DrawHudText(World world)
        {
            textRenderer.Clear(Color.Transparent);

            textRenderer.DrawString("Corrino HUD", titleFont, accentBrush, new PointF(24, 20));
            textRenderer.DrawString("LMB: select    RMB: move    X + LMB: place building", bodyFont, whiteBrush, new PointF(24, 56));
            textRenderer.DrawString("Current mode: RTS prototype", bodyFont, dimBrush, new PointF(24, 80));

            string selectedTitle = "Selected: None";
            string selectedDesc = "No actor selected";
            string buildHint = "Build panel: unavailable";
            if (world.SelectedActor != null)
            {
                selectedTitle = "Selected: " + world.GetSelectedActorDisplayName();
                selectedDesc = world.GetSelectedActorDescription();
                buildHint = world.SelectedActor.HasField("ProvideBuildings")
                    ? "Build panel: available"
                    : "Build panel: unavailable";
            }

            float bottomY = viewportSize.Y - 116;
            textRenderer.DrawString(selectedTitle, titleFont, whiteBrush, new PointF(24, bottomY));
            textRenderer.DrawString(selectedDesc, bodyFont, dimBrush, new PointF(24, bottomY + 32));
            textRenderer.DrawString(buildHint, bodyFont, accentBrush, new PointF(24, bottomY + 64));

            textRenderer.DrawString("Camera", titleFont, whiteBrush, new PointF(viewportSize.X - 284, 20));
            textRenderer.DrawString($"Actors: {world.ActorCount}", bodyFont, whiteBrush, new PointF(viewportSize.X - 284, 54));
            textRenderer.DrawString($"Buildable: {world.GetBuildableActors().Count()}", bodyFont, dimBrush, new PointF(viewportSize.X - 284, 76));
        }

        private void DrawTextOverlay()
        {
            int texture = textRenderer.Texture;
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Color4(Color.White);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0); GL.Vertex2(viewportSize.X, 0);
            GL.TexCoord2(1, 1); GL.Vertex2(viewportSize.X, viewportSize.Y);
            GL.TexCoord2(0, 1); GL.Vertex2(0, viewportSize.Y);
            GL.End();
        }

        private static void DrawRect(float x, float y, float width, float height, Color color)
        {
            GL.Color4(color);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(x, y);
            GL.Vertex2(x + width, y);
            GL.Vertex2(x + width, y + height);
            GL.Vertex2(x, y + height);
            GL.End();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            textRenderer.Dispose();
            titleFont.Dispose();
            bodyFont.Dispose();
            (accentBrush as IDisposable)?.Dispose();
            (dimBrush as IDisposable)?.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
