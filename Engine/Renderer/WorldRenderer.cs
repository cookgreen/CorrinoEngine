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
using System.Drawing;
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

        public void Render(Camera camera, IEnumerable<Actor> actors = null, IEnumerable<Actor> selectedActors = null, Actor pendingPlacementActor = null, Vector3 pendingPlacementPosition = default, Vector2i pendingPlacementFootprint = default, float placementTileSize = 48f, bool isPendingPlacementValid = false, IReadOnlyList<Vector3> movePathPoints = null, bool showDebugHitOverlay = false, bool hasDebugTerrainCursorPoint = false, Vector3 debugTerrainCursorPoint = default, bool hasDebugFlatCursorPoint = false, Vector3 debugFlatCursorPoint = default, bool hasDebugLastCommandTarget = false, Vector3 debugLastCommandTarget = default, Actor debugHoveredActor = null, IReadOnlyList<RectangleF> debugBuildingBounds = null, RectangleF? debugHoveredBuildingBounds = null)
        {
            foreach(var renderableObject in renderableObjects)
            {
                renderableObject.Draw(camera);
            }

            foreach (Actor actor in actors ?? Enumerable.Empty<Actor>())
            {
                if (actor.IsUnderConstruction)
                {
                    DrawConstructionEffect(camera, actor);
                }
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

            if (showDebugHitOverlay)
            {
                DrawDebugHitOverlay(
                    camera,
                    debugBuildingBounds,
                    debugHoveredBuildingBounds,
                    debugHoveredActor,
                    hasDebugTerrainCursorPoint,
                    debugTerrainCursorPoint,
                    hasDebugFlatCursorPoint,
                    debugFlatCursorPoint,
                    hasDebugLastCommandTarget,
                    debugLastCommandTarget);
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
            OpenTK.Graphics.OpenGL.GL.LineWidth(3f);
            OpenTK.Graphics.OpenGL.GL.Color3(0.26f, 0.92f, 0.38f);

            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip);
            foreach (Vector3 point in points)
            {
                OpenTK.Graphics.OpenGL.GL.Vertex3(point.X, point.Y, point.Z);
            }
            OpenTK.Graphics.OpenGL.GL.End();

            Vector3 target = points[points.Count - 1];
            OpenTK.Graphics.OpenGL.GL.LineWidth(2f);
            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                OpenTK.Graphics.OpenGL.GL.Vertex3(target.X + MathF.Cos(angle) * 12f, target.Y, target.Z + MathF.Sin(angle) * 12f);
            }
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }

        private void DrawConstructionEffect(Camera camera, Actor actor)
        {
            Vector3 worldPosition = actor.Position;
            float radius = Math.Max(24f, actor.SelectionRadius * 0.9f);
            float height = 16f + 64f * actor.ConstructionProgress01;
            Vector3 color = new Vector3(0.95f, 0.76f, 0.22f);

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
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + 2f, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + 2f, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + 2f, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + 2f, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + height, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + height, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + height, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + height, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + 2f, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + height, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + 2f, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + height, worldPosition.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + 2f, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X + radius, worldPosition.Y + height, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + 2f, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(worldPosition.X - radius, worldPosition.Y + height, worldPosition.Z + radius);
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }

        private void DrawDebugHitOverlay(Camera camera, IReadOnlyList<RectangleF> debugBuildingBounds, RectangleF? debugHoveredBuildingBounds, Actor debugHoveredActor, bool hasDebugTerrainCursorPoint, Vector3 debugTerrainCursorPoint, bool hasDebugFlatCursorPoint, Vector3 debugFlatCursorPoint, bool hasDebugLastCommandTarget, Vector3 debugLastCommandTarget)
        {
            foreach (RectangleF bounds in debugBuildingBounds ?? Array.Empty<RectangleF>())
            {
                DrawWorldRectangle(camera, bounds, new Vector3(0.92f, 0.8f, 0.18f), 3f);
            }

            if (debugHoveredBuildingBounds.HasValue)
            {
                DrawWorldRectangle(camera, debugHoveredBuildingBounds.Value, new Vector3(0.16f, 0.86f, 0.92f), 5f);
            }

            if (debugHoveredActor != null)
            {
                DrawSelectionRing(camera, debugHoveredActor.Position, Math.Max(14f, debugHoveredActor.SelectionRadius * 0.75f), new Vector3(0.16f, 0.86f, 0.92f));
            }

            if (hasDebugTerrainCursorPoint)
            {
                DrawDebugMarker(camera, debugTerrainCursorPoint, 10f, new Vector3(0.9f, 0.2f, 0.9f), 28f);
            }

            if (hasDebugFlatCursorPoint)
            {
                DrawDebugMarker(camera, debugFlatCursorPoint, 7f, new Vector3(0.96f, 0.54f, 0.12f), 20f);
            }

            if (hasDebugLastCommandTarget)
            {
                DrawDebugMarker(camera, debugLastCommandTarget, 14f, new Vector3(0.22f, 0.55f, 0.96f), 36f);
            }
        }

        private void DrawWorldRectangle(Camera camera, RectangleF rect, Vector3 color, float y)
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
            OpenTK.Graphics.OpenGL.GL.Vertex3(rect.Left, y, rect.Top);
            OpenTK.Graphics.OpenGL.GL.Vertex3(rect.Right, y, rect.Top);
            OpenTK.Graphics.OpenGL.GL.Vertex3(rect.Right, y, rect.Bottom);
            OpenTK.Graphics.OpenGL.GL.Vertex3(rect.Left, y, rect.Bottom);
            OpenTK.Graphics.OpenGL.GL.End();
            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }

        private void DrawDebugMarker(Camera camera, Vector3 point, float radius, Vector3 color, float height)
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
            for (int i = 0; i < 28; i++)
            {
                float angle = MathHelper.TwoPi * i / 28f;
                OpenTK.Graphics.OpenGL.GL.Vertex3(point.X + MathF.Cos(angle) * radius, point.Y + 2f, point.Z + MathF.Sin(angle) * radius);
            }
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X, point.Y + 2f, point.Z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X, point.Y + height, point.Z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X - radius, point.Y + 2f, point.Z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X + radius, point.Y + 2f, point.Z);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X, point.Y + 2f, point.Z - radius);
            OpenTK.Graphics.OpenGL.GL.Vertex3(point.X, point.Y + 2f, point.Z + radius);
            OpenTK.Graphics.OpenGL.GL.End();

            OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            OpenTK.Graphics.OpenGL.GL.PopMatrix();
            OpenTK.Graphics.OpenGL.GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        }
	}
}
