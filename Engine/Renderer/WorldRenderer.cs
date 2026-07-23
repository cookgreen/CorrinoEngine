using CorrinoEngine.Cameras;
using CorrinoEngine.Core;
using CorrinoEngine.Graphics.Mesh;
using CorrinoEngine.UI;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine.Renderer
{
    public class WorldRenderer
    {
        private static WorldRenderer instance;
        public static WorldRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WorldRenderer();
                }
                return instance;
            }
        }

        private Stack<IRenderable> renderableObjects;
        public Stack<IRenderable> RenderableObjects
        {
            get { return renderableObjects; }
        }

        public WorldRenderer()
        {
            renderableObjects = new Stack<IRenderable>();
            instance = this;
        }

        public void RenderObject(IRenderable renderableObject)
        {
            renderableObjects.Push(renderableObject);
        }

        public void Render(Camera camera, IEnumerable<Actor> actors = null, IEnumerable<Actor> selectedActors = null, Actor pendingPlacementActor = null, Vector3 pendingPlacementPosition = default, Vector2i pendingPlacementFootprint = default, float placementTileSize = 48f, bool isPendingPlacementValid = false, IReadOnlyList<Vector3> movePathPoints = null)
        {
            foreach(var renderableObject in renderableObjects)
            {
                renderableObject.Draw(camera);
            }

            foreach (Actor selectedActor in selectedActors ?? Enumerable.Empty<Actor>())
            {
                DrawSelectionRing(camera, selectedActor.Position, selectedActor.SelectionRadius, new Vector3(0.26f, 0.92f, 0.38f));
            }

            if (pendingPlacementActor != null)
            {
                DrawGhostPlacement(camera, pendingPlacementActor, pendingPlacementPosition);
                DrawPlacementFootprint(camera, pendingPlacementPosition, pendingPlacementFootprint, placementTileSize, isPendingPlacementValid);
                DrawSelectionRing(
                    camera,
                    pendingPlacementPosition,
                    70f,
                    isPendingPlacementValid ? new Vector3(0.38f, 0.92f, 0.54f) : new Vector3(0.92f, 0.32f, 0.32f));
            }

            if (movePathPoints != null && movePathPoints.Count > 0)
            {
                DrawMovePath(camera, movePathPoints);
            }
        }

        public void UpdateFrame(FrameEventArgs args)
        {
            foreach(var renderableObject in renderableObjects)
            {
                renderableObject.Update((float)args.Time);
            }
        }

		public void RemoveCurrent()
		{
            if (renderableObjects.Count == 0)
                return;

            renderableObjects.Pop();
		}

        private void DrawSelectionRing(Camera camera, Vector3 worldPosition, float radius, Vector3 color)
        {
            OpenTK.Graphics.OpenGL.GL.UseProgram(0);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.Projection);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.View);

            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.Texture2D);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.CullFace);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.LineWidth(2f);
            OpenTK.Graphics.OpenGL.GL.Color3(color.X, color.Y, color.Z);
            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float x = worldPosition.X + MathF.Cos(angle) * radius;
                float z = worldPosition.Z + MathF.Sin(angle) * radius;
                OpenTK.Graphics.OpenGL.GL.Vertex3(x, worldPosition.Y + 2f, z);
            }
            OpenTK.Graphics.OpenGL.GL.End();
            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);

            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }

        private void DrawGhostPlacement(Camera camera, Actor pendingPlacementActor, Vector3 pendingPlacementPosition)
        {
            if (pendingPlacementActor?.MeshInstance == null)
            {
                return;
            }

            Vector3 originalPosition = pendingPlacementActor.MeshInstance.Position;
            pendingPlacementActor.MeshInstance.Position = pendingPlacementPosition;
            pendingPlacementActor.MeshInstance.Draw(camera);

            pendingPlacementActor.MeshInstance.Position = originalPosition;
        }

        private void DrawPlacementFootprint(Camera camera, Vector3 centerPosition, Vector2i footprint, float tileSize, bool isValid)
        {
            if (footprint.X <= 0 || footprint.Y <= 0)
            {
                return;
            }

            Vector3 color = isValid ? new Vector3(0.38f, 0.92f, 0.54f) : new Vector3(0.92f, 0.32f, 0.32f);
            int startX = (int)MathF.Round((centerPosition.X - tileSize * 0.5f) / tileSize) - footprint.X / 2;
            int startY = (int)MathF.Round((centerPosition.Z - tileSize * 0.5f) / tileSize) - footprint.Y / 2;

            for (int z = 0; z < footprint.Y; z++)
            {
                for (int x = 0; x < footprint.X; x++)
                {
                    float worldX = (startX + x) * tileSize;
                    float worldZ = (startY + z) * tileSize;
                    DrawFootprintCell(camera, worldX, worldZ, tileSize, color);
                }
            }
        }

        private void DrawFootprintCell(Camera camera, float x, float z, float size, Vector3 color)
        {
            OpenTK.Graphics.OpenGL.GL.UseProgram(0);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.Projection);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.View);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.Texture2D);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.CullFace);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.LineWidth(1.5f);
            OpenTK.Graphics.OpenGL.GL.Color3(color.X, color.Y, color.Z);
            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
            OpenTK.Graphics.OpenGL.GL.Vertex3(x, 2f, z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(x + size, 2f, z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(x + size, 2f, z + size);
            OpenTK.Graphics.OpenGL.GL.Vertex3(x, 2f, z + size);
            OpenTK.Graphics.OpenGL.GL.End();
            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }

        private void DrawMovePath(Camera camera, IReadOnlyList<Vector3> points)
        {
            if (points.Count == 0)
                return;

            OpenTK.Graphics.OpenGL.GL.UseProgram(0);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.Projection);
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
            OpenTK.Graphics.OpenGL.GL.PushMatrix();
            OpenTK.Graphics.OpenGL.GL.LoadMatrix(ref camera.View);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.Texture2D);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.CullFace);
            OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.LineWidth(2f);
            OpenTK.Graphics.OpenGL.GL.Color3(0.26f, 0.92f, 0.38f);

            foreach (Vector3 point in points)
            {
                OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    OpenTK.Graphics.OpenGL.GL.Vertex3(point.X + MathF.Cos(angle) * 12f, point.Y + 2f, point.Z + MathF.Sin(angle) * 12f);
                }
                OpenTK.Graphics.OpenGL.GL.End();
            }

            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }
	}
}
