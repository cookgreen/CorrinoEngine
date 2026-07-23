using System;
using CorrinoEngine.Core;
using CorrinoEngine.Fields;
using CorrinoEngine.UI;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;
using System.Linq;

namespace CorrinoEngine.Renderer
{
    public class HudRenderer : IDisposable
    {
        private const float BuildPanelWidth = 430f;
        private const float BuildPanelHeight = 220f;
        private const float BuildPanelPadding = 14f;
        private const float BuildItemHeight = 28f;
        private const float QueueItemHeight = 24f;
        private const float BuildButtonWidth = 92f;
        private const float BuildButtonHeight = 28f;
        private const float CancelButtonWidth = 92f;
        private const float CancelButtonHeight = 28f;
        private const float PagerButtonWidth = 26f;
        private const int VisibleBuildItems = 5;
        private const int VisibleQueueItems = 5;

        private Vector2 viewportSize;
        private TextRenderer textRenderer;
        private readonly Font titleFont;
        private readonly Font bodyFont;
        private readonly Font smallFont;
        private readonly Brush whiteBrush;
        private readonly Brush accentBrush;
        private readonly Brush dimBrush;
        private Brush warningBrush;
        private bool disposed;
        private int buildPageIndex;
        private int queuePageIndex;
        private float lastScrollValue;
        private int hoveredBuildIndex;
        private int hoveredQueueIndex;
        private bool isHoveringBuildButton;
        private bool isHoveringCancelButton;
        private bool isHoveringBuildPrevButton;
        private bool isHoveringBuildNextButton;
        private bool isHoveringQueuePrevButton;
        private bool isHoveringQueueNextButton;

        public HudRenderer(Vector2 viewportSize)
        {
            this.viewportSize = viewportSize;
            textRenderer = new TextRenderer((int)viewportSize.X, (int)viewportSize.Y);
            titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            bodyFont = new Font("Segoe UI", 10, FontStyle.Regular);
            smallFont = new Font("Segoe UI", 9, FontStyle.Regular);
            whiteBrush = Brushes.White;
            accentBrush = new SolidBrush(Color.FromArgb(255, 220, 186, 109));
            dimBrush = new SolidBrush(Color.FromArgb(255, 180, 180, 180));
            warningBrush = new SolidBrush(Color.FromArgb(255, 225, 108, 108));
            buildPageIndex = 0;
            queuePageIndex = 0;
            lastScrollValue = 0;
            hoveredBuildIndex = -1;
            hoveredQueueIndex = -1;
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
            DrawBuildPanel(world);

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
            string modeText = world.IsInBuildingPlacementMode
                ? $"Current mode: placing {world.PendingPlacementActorTypeName} (LMB confirm / RMB cancel)"
                : "Current mode: RTS prototype";
            textRenderer.DrawString(modeText, bodyFont, dimBrush, new PointF(24, 80));

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
            DrawBuildPanelText(world);
        }

        public bool TryHandleLeftClick(World world, Vector2 mousePosition)
        {
            if (!UIManager.Instance.IsBuildQueueVisible)
            {
                return false;
            }

            RectangleF panelRect = GetBuildPanelRect();
            if (!panelRect.Contains(mousePosition.X, mousePosition.Y))
            {
                return false;
            }

            RectangleF buttonRect = GetBuildButtonRect();
            if (buttonRect.Contains(mousePosition.X, mousePosition.Y))
            {
                if (!string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName) && world.CanAffordSelectedBuild())
                {
                    world.EnqueueBuild(world.SelectedBuildActorTypeName);
                    return true;
                }

                return false;
            }

            RectangleF cancelRect = GetCancelButtonRect();
            if (cancelRect.Contains(mousePosition.X, mousePosition.Y))
            {
                world.CancelSelectedProduction();
                return true;
            }

            RectangleF buildPrevRect = GetBuildPrevButtonRect();
            if (buildPrevRect.Contains(mousePosition.X, mousePosition.Y))
            {
                buildPageIndex = Math.Max(0, buildPageIndex - 1);
                return true;
            }

            RectangleF buildNextRect = GetBuildNextButtonRect();
            if (buildNextRect.Contains(mousePosition.X, mousePosition.Y))
            {
                buildPageIndex = Math.Min(GetBuildPageCount(world) - 1, buildPageIndex + 1);
                return true;
            }

            RectangleF prevRect = GetQueuePrevButtonRect();
            if (prevRect.Contains(mousePosition.X, mousePosition.Y))
            {
                queuePageIndex = Math.Max(0, queuePageIndex - 1);
                return true;
            }

            RectangleF nextRect = GetQueueNextButtonRect();
            if (nextRect.Contains(mousePosition.X, mousePosition.Y))
            {
                queuePageIndex = Math.Min(GetQueuePageCount(world) - 1, queuePageIndex + 1);
                return true;
            }

            int index = GetBuildItemIndexAt(mousePosition);
            if (index >= 0)
            {
                var buildableActors = GetVisibleBuildActors(world);
                if (index < buildableActors.Count)
                {
                    world.SelectBuildActor(buildableActors[index].TypeName);
                }

                return true;
            }

            int queueIndex = GetQueueItemIndexAt(mousePosition, world);
            if (queueIndex >= 0)
            {
                var queue = GetVisibleQueue(world);
                if (queueIndex < queue.Count)
                {
                    world.CancelSelectedProduction(queue[queueIndex].Id);
                }

                return true;
            }

            return true;
        }

        public void HandleScroll(World world, Vector2 mousePosition, float currentScrollValue)
        {
            if (!UIManager.Instance.IsBuildQueueVisible)
            {
                lastScrollValue = currentScrollValue;
                return;
            }

            RectangleF panelRect = GetBuildPanelRect();
            if (!panelRect.Contains(mousePosition.X, mousePosition.Y))
            {
                lastScrollValue = currentScrollValue;
                return;
            }

            UpdateHoverState(world, mousePosition);
            float delta = currentScrollValue - lastScrollValue;
            if (delta > 0.01f)
            {
                if (IsBuildColumn(mousePosition))
                {
                    buildPageIndex = Math.Max(0, buildPageIndex - 1);
                }
                else
                {
                    queuePageIndex = Math.Max(0, queuePageIndex - 1);
                }
            }
            else if (delta < -0.01f)
            {
                if (IsBuildColumn(mousePosition))
                {
                    buildPageIndex = Math.Min(GetBuildPageCount(world) - 1, buildPageIndex + 1);
                }
                else
                {
                    queuePageIndex = Math.Min(GetQueuePageCount(world) - 1, queuePageIndex + 1);
                }
            }

            lastScrollValue = currentScrollValue;
        }

        public void UpdateInteraction(World world, Vector2 mousePosition)
        {
            if (!UIManager.Instance.IsBuildQueueVisible)
            {
                ClearHoverState();
                return;
            }

            UpdateHoverState(world, mousePosition);
            HandleBuildShortcuts(world);
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

        private void DrawBuildPanel(World world)
        {
            if (!UIManager.Instance.IsBuildQueueVisible)
            {
                return;
            }

            RectangleF panel = GetBuildPanelRect();
            ClampBuildPage(world);
            ClampQueuePage(world);
            DrawRect(panel.X, panel.Y, panel.Width, panel.Height, Color.FromArgb(210, 12, 16, 22));
            DrawRect(panel.X, panel.Y, panel.Width, 4, Color.FromArgb(220, 196, 160, 92));

            var buildableActors = GetVisibleBuildActors(world);
            for (int i = 0; i < buildableActors.Count; i++)
            {
                RectangleF row = GetBuildItemRect(i);
                bool isSelected = string.Equals(world.SelectedBuildActorTypeName, buildableActors[i].TypeName, StringComparison.OrdinalIgnoreCase);
                bool isHovered = i == hoveredBuildIndex;
                DrawRect(
                    row.X,
                    row.Y,
                    row.Width,
                    row.Height,
                    isSelected
                        ? Color.FromArgb(220, 63, 79, 96)
                        : isHovered
                            ? Color.FromArgb(205, 46, 57, 68)
                            : Color.FromArgb(185, 26, 33, 40));
            }

            var queue = GetVisibleQueue(world);
            for (int i = 0; i < queue.Count; i++)
            {
                RectangleF row = GetQueueItemRect(i);
                bool isHead = IsVisibleQueueHead(world, queue[i]);
                bool isHovered = i == hoveredQueueIndex;
                DrawRect(
                    row.X,
                    row.Y,
                    row.Width,
                    row.Height,
                    isHead
                        ? Color.FromArgb(205, 77, 64, 36)
                        : isHovered
                            ? Color.FromArgb(195, 49, 56, 64)
                            : Color.FromArgb(180, 31, 34, 39));
            }

            RectangleF button = GetBuildButtonRect();
            Color buttonColor = string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName)
                ? Color.FromArgb(120, 55, 55, 55)
                : world.CanAffordSelectedBuild()
                    ? isHoveringBuildButton ? Color.FromArgb(235, 144, 109, 44) : Color.FromArgb(220, 126, 97, 40)
                    : Color.FromArgb(150, 88, 54, 54);
            DrawRect(button.X, button.Y, button.Width, button.Height, buttonColor);

            RectangleF cancelButton = GetCancelButtonRect();
            Color cancelColor = world.SelectedProductionQueueCount > 0
                ? isHoveringCancelButton ? Color.FromArgb(235, 142, 61, 61) : Color.FromArgb(220, 120, 54, 54)
                : Color.FromArgb(120, 55, 55, 55);
            DrawRect(cancelButton.X, cancelButton.Y, cancelButton.Width, cancelButton.Height, cancelColor);

            DrawBuildPager(world);
            DrawQueuePager(world);

            DrawProductionProgress(world);
        }

        private void DrawBuildPanelText(World world)
        {
            if (!UIManager.Instance.IsBuildQueueVisible)
            {
                return;
            }

            RectangleF panel = GetBuildPanelRect();
            ClampBuildPage(world);
            ClampQueuePage(world);
            textRenderer.DrawString("Build Queue", titleFont, whiteBrush, new PointF(panel.X + BuildPanelPadding, panel.Y + 10));
            textRenderer.DrawString("Click or press 1-5 to select, Enter to build.", smallFont, dimBrush, new PointF(panel.X + BuildPanelPadding, panel.Y + 38));
            textRenderer.DrawString($"Credits: {world.Credits}", bodyFont, accentBrush, new PointF(panel.Right - 140, panel.Y + 12));

            textRenderer.DrawString($"Build {buildPageIndex + 1}/{GetBuildPageCount(world)}", smallFont, dimBrush, new PointF(panel.X + 110, panel.Y + 14));
            RectangleF buildPrev = GetBuildPrevButtonRect();
            RectangleF buildNext = GetBuildNextButtonRect();
            textRenderer.DrawString("<", bodyFont, whiteBrush, new PointF(buildPrev.X + 8, buildPrev.Y + 1));
            textRenderer.DrawString(">", bodyFont, whiteBrush, new PointF(buildNext.X + 8, buildNext.Y + 1));

            var buildableActors = GetVisibleBuildActors(world);
            for (int i = 0; i < buildableActors.Count; i++)
            {
                ActorData actorData = buildableActors[i];
                RectangleF row = GetBuildItemRect(i);
                string label = world.GetActorDisplayName(actorData);
                string desc = $"{world.GetActorCost(actorData.TypeName)} cr";
                textRenderer.DrawString($"{i + 1}. {label}", bodyFont, whiteBrush, new PointF(row.X + 8, row.Y + 4));
                textRenderer.DrawString(desc, smallFont, dimBrush, new PointF(row.Right - 52, row.Y + 6));
            }

            textRenderer.DrawString("Queue", bodyFont, whiteBrush, new PointF(GetQueuePanelLeft(), panel.Y + 38));
            textRenderer.DrawString($"{queuePageIndex + 1}/{GetQueuePageCount(world)}", smallFont, dimBrush, new PointF(GetQueuePanelLeft() + 80, panel.Y + 41));
            RectangleF prev = GetQueuePrevButtonRect();
            RectangleF next = GetQueueNextButtonRect();
            textRenderer.DrawString("<", bodyFont, whiteBrush, new PointF(prev.X + 8, prev.Y + 1));
            textRenderer.DrawString(">", bodyFont, whiteBrush, new PointF(next.X + 8, next.Y + 1));
            var queue = GetVisibleQueue(world);
            for (int i = 0; i < queue.Count; i++)
            {
                ProductionOrder order = queue[i];
                RectangleF row = GetQueueItemRect(i);
                ActorData actorData = world.GetActorData(order.ActorTypeName);
                string label = world.GetActorDisplayName(actorData);
                bool isGlobalHead = IsVisibleQueueHead(world, order);
                string suffix = isGlobalHead ? $"{(int)(world.GetSelectedProductionProgress01() * 100)}%" : "Cancel";
                textRenderer.DrawString(label, smallFont, whiteBrush, new PointF(row.X + 8, row.Y + 5));
                textRenderer.DrawString(suffix, smallFont, dimBrush, new PointF(row.Right - 42, row.Y + 5));
            }

            RectangleF button = GetBuildButtonRect();
            textRenderer.DrawString("Build", bodyFont, whiteBrush, new PointF(button.X + 24, button.Y + 5));
            RectangleF cancelButton = GetCancelButtonRect();
            textRenderer.DrawString("Cancel", bodyFont, whiteBrush, new PointF(cancelButton.X + 18, cancelButton.Y + 5));

            string selectedBuild = string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName)
                ? "No build selected"
                : "Queued: " + world.SelectedBuildActorTypeName;
            textRenderer.DrawString(selectedBuild, smallFont, accentBrush, new PointF(panel.X + BuildPanelPadding, panel.Bottom - 26));

            ProductionOrder currentProduction = world.GetSelectedProduction();
            string queueStatus = currentProduction == null
                ? "Production: idle"
                : $"Production: {world.GetActorDisplayName(world.GetActorData(currentProduction.ActorTypeName))}";
            textRenderer.DrawString(queueStatus, smallFont, whiteBrush, new PointF(panel.X + BuildPanelPadding, panel.Bottom - 50));
            textRenderer.DrawString($"Queue: {world.SelectedProductionQueueCount}", smallFont, dimBrush, new PointF(panel.X + 240, panel.Bottom - 50));

            if (!string.IsNullOrWhiteSpace(world.BuildFeedbackMessage))
            {
                warningBrush?.Dispose();
                warningBrush = new SolidBrush(Color.FromArgb((int)(255 * world.BuildFeedbackAlpha), 225, 108, 108));
                textRenderer.DrawString(world.BuildFeedbackMessage, smallFont, warningBrush, new PointF(panel.X + BuildPanelPadding, panel.Bottom - 72));
            }
        }

        private RectangleF GetBuildPanelRect()
        {
            return new RectangleF(
                viewportSize.X - BuildPanelWidth - 16,
                viewportSize.Y - BuildPanelHeight - 16,
                BuildPanelWidth,
                BuildPanelHeight);
        }

        private RectangleF GetBuildItemRect(int index)
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(
                panel.X + BuildPanelPadding,
                panel.Y + 62 + index * (BuildItemHeight + 6),
                186,
                BuildItemHeight);
        }

        private RectangleF GetBuildButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(
                panel.Right - BuildPanelPadding - BuildButtonWidth,
                panel.Bottom - BuildPanelPadding - BuildButtonHeight,
                BuildButtonWidth,
                BuildButtonHeight);
        }

        private RectangleF GetCancelButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(
                panel.Right - BuildPanelPadding - BuildButtonWidth - CancelButtonWidth - 8,
                panel.Bottom - BuildPanelPadding - CancelButtonHeight,
                CancelButtonWidth,
                CancelButtonHeight);
        }

        private int GetBuildItemIndexAt(Vector2 mousePosition)
        {
            int visibleCount = GetVisibleBuildActorsCacheCount;
            for (int i = 0; i < visibleCount; i++)
            {
                if (GetBuildItemRect(i).Contains(mousePosition.X, mousePosition.Y))
                {
                    return i;
                }
            }

            return -1;
        }

        private RectangleF GetQueueItemRect(int index)
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(
                GetQueuePanelLeft(),
                panel.Y + 62 + index * (QueueItemHeight + 6),
                panel.Right - GetQueuePanelLeft() - BuildPanelPadding,
                QueueItemHeight);
        }

        private float GetQueuePanelLeft()
        {
            RectangleF panel = GetBuildPanelRect();
            return panel.X + 214f;
        }

        private int GetQueueItemIndexAt(Vector2 mousePosition, World world)
        {
            int visibleCount = GetVisibleQueue(world).Count;
            for (int i = 0; i < visibleCount; i++)
            {
                if (GetQueueItemRect(i).Contains(mousePosition.X, mousePosition.Y))
                {
                    return i;
                }
            }

            return -1;
        }

        private RectangleF GetQueuePrevButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(panel.Right - 64, panel.Y + 36, PagerButtonWidth, 20);
        }

        private RectangleF GetQueueNextButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(panel.Right - 34, panel.Y + 36, PagerButtonWidth, 20);
        }

        private void DrawQueuePager(World world)
        {
            RectangleF prev = GetQueuePrevButtonRect();
            RectangleF next = GetQueueNextButtonRect();
            bool canPrev = queuePageIndex > 0;
            bool canNext = queuePageIndex < GetQueuePageCount(world) - 1;

            DrawRect(prev.X, prev.Y, prev.Width, prev.Height, canPrev ? (isHoveringQueuePrevButton ? Color.FromArgb(210, 72, 84, 94) : Color.FromArgb(180, 55, 65, 75)) : Color.FromArgb(100, 45, 45, 45));
            DrawRect(next.X, next.Y, next.Width, next.Height, canNext ? (isHoveringQueueNextButton ? Color.FromArgb(210, 72, 84, 94) : Color.FromArgb(180, 55, 65, 75)) : Color.FromArgb(100, 45, 45, 45));
        }

        private RectangleF GetBuildPrevButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(panel.X + 160, panel.Y + 10, PagerButtonWidth, 20);
        }

        private RectangleF GetBuildNextButtonRect()
        {
            RectangleF panel = GetBuildPanelRect();
            return new RectangleF(panel.X + 190, panel.Y + 10, PagerButtonWidth, 20);
        }

        private void DrawBuildPager(World world)
        {
            RectangleF prev = GetBuildPrevButtonRect();
            RectangleF next = GetBuildNextButtonRect();
            bool canPrev = buildPageIndex > 0;
            bool canNext = buildPageIndex < GetBuildPageCount(world) - 1;

            DrawRect(prev.X, prev.Y, prev.Width, prev.Height, canPrev ? (isHoveringBuildPrevButton ? Color.FromArgb(210, 72, 84, 94) : Color.FromArgb(180, 55, 65, 75)) : Color.FromArgb(100, 45, 45, 45));
            DrawRect(next.X, next.Y, next.Width, next.Height, canNext ? (isHoveringBuildNextButton ? Color.FromArgb(210, 72, 84, 94) : Color.FromArgb(180, 55, 65, 75)) : Color.FromArgb(100, 45, 45, 45));
        }

        private int GetBuildPageCount(World world)
        {
            int count = world.GetBuildableActors().Count();
            return Math.Max(1, (int)Math.Ceiling(count / (float)VisibleBuildItems));
        }

        private void ClampBuildPage(World world)
        {
            buildPageIndex = Math.Clamp(buildPageIndex, 0, GetBuildPageCount(world) - 1);
        }

        private System.Collections.Generic.List<ActorData> GetVisibleBuildActors(World world)
        {
            ClampBuildPage(world);
            var visible = world.GetBuildableActors()
                .Skip(buildPageIndex * VisibleBuildItems)
                .Take(VisibleBuildItems)
                .ToList();
            GetVisibleBuildActorsCacheCount = visible.Count;
            return visible;
        }

        private int GetQueuePageCount(World world)
        {
            int count = world.GetSelectedProductionQueue().Count;
            return Math.Max(1, (int)Math.Ceiling(count / (float)VisibleQueueItems));
        }

        private void ClampQueuePage(World world)
        {
            queuePageIndex = Math.Clamp(queuePageIndex, 0, GetQueuePageCount(world) - 1);
        }

        private System.Collections.Generic.List<ProductionOrder> GetVisibleQueue(World world)
        {
            ClampQueuePage(world);
            return world.GetSelectedProductionQueue()
                .Skip(queuePageIndex * VisibleQueueItems)
                .Take(VisibleQueueItems)
                .ToList();
        }

        private bool IsVisibleQueueHead(World world, ProductionOrder order)
        {
            ProductionOrder head = world.GetSelectedProduction();
            return head != null && order != null && head.Id == order.Id;
        }

        private int GetVisibleBuildActorsCacheCount { get; set; }

        private void HandleBuildShortcuts(World world)
        {
            var visibleBuildActors = GetVisibleBuildActors(world);
            if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D1) && visibleBuildActors.Count > 0)
            {
                world.SelectBuildActor(visibleBuildActors[0].TypeName);
            }
            else if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D2) && visibleBuildActors.Count > 1)
            {
                world.SelectBuildActor(visibleBuildActors[1].TypeName);
            }
            else if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D3) && visibleBuildActors.Count > 2)
            {
                world.SelectBuildActor(visibleBuildActors[2].TypeName);
            }
            else if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D4) && visibleBuildActors.Count > 3)
            {
                world.SelectBuildActor(visibleBuildActors[3].TypeName);
            }
            else if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D5) && visibleBuildActors.Count > 4)
            {
                world.SelectBuildActor(visibleBuildActors[4].TypeName);
            }

            if (world.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter)
                && !string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName)
                && world.CanAffordSelectedBuild())
            {
                world.EnqueueBuild(world.SelectedBuildActorTypeName);
            }
        }

        private void UpdateHoverState(World world, Vector2 mousePosition)
        {
            hoveredBuildIndex = -1;
            hoveredQueueIndex = -1;
            isHoveringBuildButton = false;
            isHoveringCancelButton = false;
            isHoveringBuildPrevButton = false;
            isHoveringBuildNextButton = false;
            isHoveringQueuePrevButton = false;
            isHoveringQueueNextButton = false;

            RectangleF panelRect = GetBuildPanelRect();
            if (!panelRect.Contains(mousePosition.X, mousePosition.Y))
            {
                return;
            }

            isHoveringBuildButton = GetBuildButtonRect().Contains(mousePosition.X, mousePosition.Y);
            isHoveringCancelButton = GetCancelButtonRect().Contains(mousePosition.X, mousePosition.Y);
            isHoveringBuildPrevButton = GetBuildPrevButtonRect().Contains(mousePosition.X, mousePosition.Y);
            isHoveringBuildNextButton = GetBuildNextButtonRect().Contains(mousePosition.X, mousePosition.Y);
            isHoveringQueuePrevButton = GetQueuePrevButtonRect().Contains(mousePosition.X, mousePosition.Y);
            isHoveringQueueNextButton = GetQueueNextButtonRect().Contains(mousePosition.X, mousePosition.Y);
            hoveredBuildIndex = GetBuildItemIndexAt(mousePosition);
            hoveredQueueIndex = GetQueueItemIndexAt(mousePosition, world);
        }

        private void ClearHoverState()
        {
            hoveredBuildIndex = -1;
            hoveredQueueIndex = -1;
            isHoveringBuildButton = false;
            isHoveringCancelButton = false;
            isHoveringBuildPrevButton = false;
            isHoveringBuildNextButton = false;
            isHoveringQueuePrevButton = false;
            isHoveringQueueNextButton = false;
        }

        private bool IsBuildColumn(Vector2 mousePosition)
        {
            return mousePosition.X < GetQueuePanelLeft();
        }

        private void DrawProductionProgress(World world)
        {
            RectangleF panel = GetBuildPanelRect();
            float progressY = panel.Bottom - 62;
            float progressX = panel.X + BuildPanelPadding;
            float progressWidth = panel.Width - BuildPanelPadding * 2;
            float progressHeight = 10f;

            DrawRect(progressX, progressY, progressWidth, progressHeight, Color.FromArgb(120, 38, 42, 48));

            float fill = progressWidth * world.GetSelectedProductionProgress01();
            if (fill > 0)
            {
                DrawRect(progressX, progressY, fill, progressHeight, Color.FromArgb(220, 196, 160, 92));
            }
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
            smallFont.Dispose();
            (accentBrush as IDisposable)?.Dispose();
            (dimBrush as IDisposable)?.Dispose();
            (warningBrush as IDisposable)?.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
