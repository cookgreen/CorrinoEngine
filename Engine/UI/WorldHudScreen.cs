using CorrinoEngine.Core;
using CorrinoEngine.Fields;
using CorrinoEngine.UI.Widgets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CorrinoEngine.UI
{
    public class WorldHudScreen : UIScreen
    {
        private const float BuildPanelWidth = 500f;
        private const float BuildPanelHeight = 248f;
        private const int VisibleBuildColumns = 2;
        private const int VisibleBuildRows = 3;
        private const int VisibleBuildItems = VisibleBuildColumns * VisibleBuildRows;
        private const int VisibleQueueItems = 5;

        private readonly World world;
        private readonly PanelWidget rootPanel;
        private readonly PanelWidget topLeftPanel;
        private readonly PanelWidget bottomLeftPanel;
        private readonly PanelWidget topRightPanel;
        private readonly PanelWidget buildPanel;
        private readonly CardGridWidget<ActorData> buildGrid;
        private readonly ListWidget<ProductionOrder> queueList;
        private readonly ButtonWidget buildButton;
        private readonly ButtonWidget cancelButton;
        private readonly ButtonWidget buildPrevButton;
        private readonly ButtonWidget buildNextButton;
        private readonly ButtonWidget queuePrevButton;
        private readonly ButtonWidget queueNextButton;
        private readonly ProgressBarWidget progressBar;
        private int buildPageIndex;
        private int queuePageIndex;
        private bool isBuildPanelVisible;

        public WorldHudScreen(World world) : base("WorldHud")
        {
            this.world = world;
            BlocksWorldInput = false;

            rootPanel = new PanelWidget();
            Root = rootPanel;

            topLeftPanel = new PanelWidget { BackgroundColor = Color.FromArgb(170, 10, 14, 18) };
            bottomLeftPanel = new PanelWidget { BackgroundColor = Color.FromArgb(185, 12, 16, 20) };
            topRightPanel = new PanelWidget { BackgroundColor = Color.FromArgb(160, 10, 14, 18) };
            buildPanel = new PanelWidget { BackgroundColor = Color.FromArgb(210, 12, 16, 22), ConsumeInput = true };

            buildGrid = new CardGridWidget<ActorData>
            {
                Columns = VisibleBuildColumns,
                CardHeight = 72f,
                IconSelector = actor => actor == null ? 0 : world.GetActorIconTexture(actor.TypeName),
                TitleSelector = actor => world.GetActorDisplayName(actor),
                SubtitleSelector = actor => $"{world.GetActorCost(actor.TypeName)} cr",
                SelectedSelector = actor => string.Equals(world.SelectedBuildActorTypeName, actor.TypeName, StringComparison.OrdinalIgnoreCase),
                EnabledSelector = actor => actor != null,
                ItemClicked = actor => world.SelectBuildActor(actor.TypeName)
            };

            queueList = new ListWidget<ProductionOrder>
            {
                ItemHeight = 24f,
                IconSelector = order => order == null ? 0 : world.GetActorIconTexture(order.ActorTypeName),
                TextSelector = order =>
                {
                    ActorData actorData = world.GetActorData(order.ActorTypeName);
                    string label = world.GetActorDisplayName(actorData);
                    if (world.GetSelectedProduction()?.Id == order.Id)
                        return $"{label}  {(int)(world.GetSelectedProductionProgress01() * 100)}%";
                    return $"{label}  Cancel";
                },
                ItemClicked = order => world.CancelSelectedProduction(order.Id)
            };

            buildButton = new ButtonWidget
            {
                Text = "Build",
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                    if (!string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName) && world.CanAffordSelectedBuild())
                        world.EnqueueBuild(world.SelectedBuildActorTypeName);
                }
            };
            cancelButton = new ButtonWidget
            {
                Text = "Cancel",
                HoverColor = Color.FromArgb(235, 142, 61, 61),
                Clicked = () => world.CancelSelectedProduction()
            };
            buildPrevButton = CreatePager("<", () => buildPageIndex = Math.Max(0, buildPageIndex - 1));
            buildNextButton = CreatePager(">", () => buildPageIndex++);
            queuePrevButton = CreatePager("<", () => queuePageIndex = Math.Max(0, queuePageIndex - 1));
            queueNextButton = CreatePager(">", () => queuePageIndex++);
            progressBar = new ProgressBarWidget();

            rootPanel.AddChild(topLeftPanel);
            rootPanel.AddChild(bottomLeftPanel);
            rootPanel.AddChild(topRightPanel);
            rootPanel.AddChild(buildPanel);
            buildPanel.AddChild(buildGrid);
            buildPanel.AddChild(queueList);
            buildPanel.AddChild(buildButton);
            buildPanel.AddChild(cancelButton);
            buildPanel.AddChild(buildPrevButton);
            buildPanel.AddChild(buildNextButton);
            buildPanel.AddChild(queuePrevButton);
            buildPanel.AddChild(queueNextButton);
            buildPanel.AddChild(progressBar);
        }

        public override void Layout(RectangleF viewport)
        {
            rootPanel.Layout(viewport);
            topLeftPanel.Layout(new RectangleF(12, 12, 320, 136));
            bottomLeftPanel.Layout(new RectangleF(12, viewport.Height - 136, 420, 124));
            topRightPanel.Layout(new RectangleF(viewport.Width - 300, 12, 288, 100));
            buildPanel.Layout(new RectangleF(viewport.Width - BuildPanelWidth - 16, viewport.Height - BuildPanelHeight - 16, BuildPanelWidth, BuildPanelHeight));

            RectangleF panel = buildPanel.Bounds;
            buildGrid.Layout(new RectangleF(panel.X + 14, panel.Y + 62, 72f * VisibleBuildColumns + 10f, 72f * VisibleBuildRows + 20f));
            queueList.Layout(new RectangleF(panel.X + 202, panel.Y + 62, panel.Width - 216, 24f * VisibleQueueItems + 24f));
            buildPrevButton.Layout(new RectangleF(panel.X + 160, panel.Y + 10, 26, 20));
            buildNextButton.Layout(new RectangleF(panel.X + 190, panel.Y + 10, 26, 20));
            queuePrevButton.Layout(new RectangleF(panel.Right - 64, panel.Y + 36, 26, 20));
            queueNextButton.Layout(new RectangleF(panel.Right - 34, panel.Y + 36, 26, 20));
            cancelButton.Layout(new RectangleF(panel.Right - 14 - 92 - 92 - 8, panel.Bottom - 14 - 28, 92, 28));
            buildButton.Layout(new RectangleF(panel.Right - 14 - 92, panel.Bottom - 14 - 28, 92, 28));
            progressBar.Layout(new RectangleF(panel.X + 14, panel.Bottom - 62, panel.Width - 28, 10));
        }

        public override void Update(UiInputState input)
        {
            IsVisible = true;
            isBuildPanelVisible = world.SelectedActor != null && world.CanActorProduce(world.SelectedActor);
            BlocksWorldInput = isBuildPanelVisible && buildPanel.HitTest(input.MousePosition);

            if (isBuildPanelVisible && Math.Abs(input.ScrollDelta) > float.Epsilon && buildPanel.HitTest(input.MousePosition))
            {
                if (input.MousePosition.X < queueList.Bounds.X)
                    buildPageIndex += input.ScrollDelta > 0 ? -1 : 1;
                else
                    queuePageIndex += input.ScrollDelta > 0 ? -1 : 1;
            }

            buildPageIndex = Math.Clamp(buildPageIndex, 0, GetBuildPageCount() - 1);
            queuePageIndex = Math.Clamp(queuePageIndex, 0, GetQueuePageCount() - 1);

            buildGrid.SetItems(GetVisibleBuildActors());
            queueList.SetItems(GetVisibleQueue());
            buildButton.Enabled = !string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName) && world.CanAffordSelectedBuild();
            cancelButton.Enabled = world.SelectedProductionQueueCount > 0;
            buildNextButton.Enabled = buildPageIndex < GetBuildPageCount() - 1;
            buildPrevButton.Enabled = buildPageIndex > 0;
            queueNextButton.Enabled = queuePageIndex < GetQueuePageCount() - 1;
            queuePrevButton.Enabled = queuePageIndex > 0;
            progressBar.Value01 = world.GetSelectedProductionProgress01();

            if (isBuildPanelVisible && input.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D1))
                SelectVisibleBuildIndex(0);
            else if (isBuildPanelVisible && input.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D2))
                SelectVisibleBuildIndex(1);
            else if (isBuildPanelVisible && input.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D3))
                SelectVisibleBuildIndex(2);
            else if (isBuildPanelVisible && input.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D4))
                SelectVisibleBuildIndex(3);
            else if (isBuildPanelVisible && input.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D5))
                SelectVisibleBuildIndex(4);

            base.Update(input);
        }

        public override void Render(UiRenderContext context)
        {
            topLeftPanel.Render(context);
            bottomLeftPanel.Render(context);
            topRightPanel.Render(context);

            context.TextRenderer?.DrawString("Corrino HUD", context.TitleFont, context.AccentBrush, new PointF(24, 20));
            context.TextRenderer?.DrawString("LMB: select    RMB: move    Shift+A: asset browser", context.BodyFont, context.WhiteBrush, new PointF(24, 56));
            string modeText = world.IsInBuildingPlacementMode
                ? $"Current mode: placing {world.PendingPlacementActorTypeName} (LMB confirm / RMB cancel)"
                : "Current mode: battlefield";
            context.TextRenderer?.DrawString(modeText, context.BodyFont, context.DimBrush, new PointF(24, 80));

            string selectedTitle = world.SelectedActor == null ? "Selected: None" : "Selected: " + world.GetSelectedActorDisplayName();
            string selectedDesc = world.GetSelectedActorDescription();
            string buildHint = world.SelectedActor != null && world.CanActorProduce(world.SelectedActor) ? "Build panel: available" : "Build panel: unavailable";
            float bottomY = context.ViewportSize.Y - 116;
            context.TextRenderer?.DrawString(selectedTitle, context.TitleFont, context.WhiteBrush, new PointF(24, bottomY));
            context.TextRenderer?.DrawString(selectedDesc, context.BodyFont, context.DimBrush, new PointF(24, bottomY + 32));
            context.TextRenderer?.DrawString(buildHint, context.BodyFont, context.AccentBrush, new PointF(24, bottomY + 64));

            context.TextRenderer?.DrawString("Camera", context.TitleFont, context.WhiteBrush, new PointF(context.ViewportSize.X - 284, 20));
            context.TextRenderer?.DrawString($"Actors: {world.ActorCount}", context.BodyFont, context.WhiteBrush, new PointF(context.ViewportSize.X - 284, 54));
            context.TextRenderer?.DrawString($"Buildable: {world.GetBuildableActors().Count()}", context.BodyFont, context.DimBrush, new PointF(context.ViewportSize.X - 284, 76));

            if (!isBuildPanelVisible)
                return;

            buildPanel.Render(context);

            RectangleF panel = buildPanel.Bounds;
            context.DrawRect?.Invoke(panel.X, panel.Y, panel.Width, 4, Color.FromArgb(220, 196, 160, 92));
            context.TextRenderer?.DrawString("Build Queue", context.TitleFont, context.WhiteBrush, new PointF(panel.X + 14, panel.Y + 10));
            context.TextRenderer?.DrawString("Click a card to select, then click Build.", context.SmallFont, context.DimBrush, new PointF(panel.X + 14, panel.Y + 38));
            context.TextRenderer?.DrawString($"Credits: {world.Credits}", context.BodyFont, context.AccentBrush, new PointF(panel.Right - 140, panel.Y + 12));
            context.TextRenderer?.DrawString($"Build {buildPageIndex + 1}/{GetBuildPageCount()}", context.SmallFont, context.DimBrush, new PointF(panel.X + 110, panel.Y + 14));
            context.TextRenderer?.DrawString($"{queuePageIndex + 1}/{GetQueuePageCount()}", context.SmallFont, context.DimBrush, new PointF(panel.Right - 108, panel.Y + 41));
            context.TextRenderer?.DrawString("Queue", context.BodyFont, context.WhiteBrush, new PointF(queueList.Bounds.X, panel.Y + 38));

            string selectedBuild = string.IsNullOrWhiteSpace(world.SelectedBuildActorTypeName)
                ? "No build selected"
                : "Selected: " + world.GetActorDisplayName(world.GetActorData(world.SelectedBuildActorTypeName));
            context.TextRenderer?.DrawString(selectedBuild, context.SmallFont, context.AccentBrush, new PointF(panel.X + 14, panel.Bottom - 26));

            ProductionOrder currentProduction = world.GetSelectedProduction();
            string queueStatus = currentProduction == null
                ? "Production: idle"
                : $"Production: {world.GetActorDisplayName(world.GetActorData(currentProduction.ActorTypeName))}";
            context.TextRenderer?.DrawString(queueStatus, context.SmallFont, context.WhiteBrush, new PointF(panel.X + 14, panel.Bottom - 50));
            context.TextRenderer?.DrawString($"Queue: {world.SelectedProductionQueueCount}", context.SmallFont, context.DimBrush, new PointF(panel.X + 240, panel.Bottom - 50));

            if (!string.IsNullOrWhiteSpace(world.BuildFeedbackMessage))
            {
                using SolidBrush warningBrush = new SolidBrush(Color.FromArgb((int)(255 * world.BuildFeedbackAlpha), 225, 108, 108));
                context.TextRenderer?.DrawString(world.BuildFeedbackMessage, context.SmallFont, warningBrush, new PointF(panel.X + 14, panel.Bottom - 72));
            }
        }

        private ButtonWidget CreatePager(string text, Action clicked)
        {
            return new ButtonWidget
            {
                Text = text,
                BackgroundColor = Color.FromArgb(180, 55, 65, 75),
                HoverColor = Color.FromArgb(210, 72, 84, 94),
                Clicked = clicked
            };
        }

        private List<ActorData> GetVisibleBuildActors()
        {
            return world.GetBuildableActors()
                .Skip(buildPageIndex * VisibleBuildItems)
                .Take(VisibleBuildItems)
                .ToList();
        }

        private List<ProductionOrder> GetVisibleQueue()
        {
            return world.GetSelectedProductionQueue()
                .Skip(queuePageIndex * VisibleQueueItems)
                .Take(VisibleQueueItems)
                .ToList();
        }

        private int GetBuildPageCount()
        {
            return Math.Max(1, (int)Math.Ceiling(world.GetBuildableActors().Count() / (float)VisibleBuildItems));
        }

        private int GetQueuePageCount()
        {
            return Math.Max(1, (int)Math.Ceiling(world.GetSelectedProductionQueue().Count / (float)VisibleQueueItems));
        }

        private void SelectVisibleBuildIndex(int index)
        {
            List<ActorData> visibleActors = GetVisibleBuildActors();
            if (index >= 0 && index < visibleActors.Count)
                world.SelectBuildActor(visibleActors[index].TypeName);
        }
    }
}
