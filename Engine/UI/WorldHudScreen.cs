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
        private const float BuildPanelWidth = 330f;
        private const float BuildPanelHeight = 340f;
        private const int VisibleBuildColumns = 3;
        private const int VisibleBuildRows = 3;
        private const int VisibleBuildItems = VisibleBuildColumns * VisibleBuildRows;
        private const string TabBuilding = "building";
        private const string TabInfantry = "infantry";
        private const string TabVehicle = "vehicle";

        private readonly World world;
        private readonly PanelWidget rootPanel;
        private readonly PanelWidget topLeftPanel;
        private readonly PanelWidget bottomLeftPanel;
        private readonly PanelWidget topRightPanel;
        private readonly PanelWidget buildPanel;
        private readonly CardGridWidget<ActorData> buildGrid;
        private readonly ButtonWidget buildingTabButton;
        private readonly ButtonWidget infantryTabButton;
        private readonly ButtonWidget vehicleTabButton;
        private int buildPageIndex;
        private bool isBuildPanelVisible;
        private string selectedTab = TabBuilding;

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
                CardHeight = 84f,
                DrawLabels = false,
                IconPadding = 4f,
                IconSelector = actor => actor == null ? 0 : world.GetActorIconTexture(actor.TypeName),
                ProgressSelector = actor => actor == null ? 0f : world.GetBuildProgressFor(actor.TypeName),
                TitleSelector = actor => string.Empty,
                SubtitleSelector = actor => string.Empty,
                TooltipSelector = actor => BuildTooltip(actor),
                SelectedSelector = actor => string.Equals(world.SelectedBuildActorTypeName, actor.TypeName, StringComparison.OrdinalIgnoreCase),
                EnabledSelector = actor => actor != null,
                ItemClicked = actor =>
                {
                    world.SelectBuildActor(actor.TypeName);
                    if (world.CanAffordSelectedBuild())
                        world.EnqueueBuild(actor.TypeName);
                }
            };
            buildingTabButton = CreateTabButton(TabBuilding, "building");
            infantryTabButton = CreateTabButton(TabInfantry, "infantry");
            vehicleTabButton = CreateTabButton(TabVehicle, "vehicle");

            rootPanel.AddChild(topLeftPanel);
            rootPanel.AddChild(bottomLeftPanel);
            rootPanel.AddChild(topRightPanel);
            rootPanel.AddChild(buildPanel);
            buildPanel.AddChild(buildingTabButton);
            buildPanel.AddChild(infantryTabButton);
            buildPanel.AddChild(vehicleTabButton);
            buildPanel.AddChild(buildGrid);
        }

        public override void Layout(RectangleF viewport)
        {
            rootPanel.Layout(viewport);
            topLeftPanel.Layout(new RectangleF(12, 12, 320, 136));
            bottomLeftPanel.Layout(new RectangleF(12, viewport.Height - 136, 420, 124));
            topRightPanel.Layout(new RectangleF(viewport.Width - 300, 12, 288, 100));
            buildPanel.Layout(new RectangleF(viewport.Width - BuildPanelWidth - 16, viewport.Height - BuildPanelHeight - 16, BuildPanelWidth, BuildPanelHeight));

            RectangleF panel = buildPanel.Bounds;
            buildingTabButton.Layout(new RectangleF(panel.X + 0, panel.Y - 34, 120, 32));
            infantryTabButton.Layout(new RectangleF(panel.X + 124, panel.Y - 34, 120, 32));
            vehicleTabButton.Layout(new RectangleF(panel.X + 248, panel.Y - 34, 120, 32));
            buildGrid.Layout(new RectangleF(panel.X + 22, panel.Y + 22, panel.Width - 44, panel.Height - 44));
        }

        public override void Update(UiInputState input)
        {
            IsVisible = true;
            isBuildPanelVisible = world.SelectedActor != null && world.CanActorProduce(world.SelectedActor);
            BlocksWorldInput = IsBuildUiHit(input.MousePosition);

            if (isBuildPanelVisible && Math.Abs(input.ScrollDelta) > float.Epsilon && IsBuildUiHit(input.MousePosition))
            {
                buildPageIndex += input.ScrollDelta > 0 ? -1 : 1;
            }

            buildPageIndex = Math.Clamp(buildPageIndex, 0, GetBuildPageCount() - 1);

            buildGrid.SetItems(GetVisibleBuildActors());
            UpdateTabVisuals();

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
            context.TextRenderer?.DrawString($"Map: {world.GetSelectedMapDisplayName()}", context.BodyFont, context.DimBrush, new PointF(24, bottomY + 88));

            context.TextRenderer?.DrawString("Camera", context.TitleFont, context.WhiteBrush, new PointF(context.ViewportSize.X - 284, 20));
            context.TextRenderer?.DrawString($"Actors: {world.ActorCount}", context.BodyFont, context.WhiteBrush, new PointF(context.ViewportSize.X - 284, 54));
            context.TextRenderer?.DrawString($"Buildable: {world.GetBuildableActors().Count()}", context.BodyFont, context.DimBrush, new PointF(context.ViewportSize.X - 284, 76));

            buildPanel.Render(context);

            RectangleF panel = buildPanel.Bounds;
            context.DrawRect?.Invoke(panel.X, panel.Y, panel.Width, 4, Color.FromArgb(220, 196, 160, 92));
            context.TextRenderer?.DrawString($"Credits: {world.Credits}", context.BodyFont, context.AccentBrush, new PointF(panel.Right - 126, panel.Y + 8));
            if (isBuildPanelVisible)
                context.TextRenderer?.DrawString($"Page {buildPageIndex + 1}/{GetBuildPageCount()}", context.SmallFont, context.DimBrush, new PointF(panel.X + 18, panel.Y + 8));
            else
                context.TextRenderer?.DrawString("Select a production building", context.SmallFont, context.DimBrush, new PointF(panel.X + 18, panel.Y + 8));

            if (!string.IsNullOrWhiteSpace(world.BuildFeedbackMessage))
            {
                using SolidBrush warningBrush = new SolidBrush(Color.FromArgb((int)(255 * world.BuildFeedbackAlpha), 225, 108, 108));
                context.TextRenderer?.DrawString(world.BuildFeedbackMessage, context.SmallFont, warningBrush, new PointF(panel.X + 18, panel.Bottom - 22));
            }

            if (world.IsSelectionDragging && world.HasSelectionRectangle())
            {
                RectangleF selectionRect = world.SelectionRectangle;
                context.DrawRect?.Invoke(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height, Color.FromArgb(70, 50, 180, 90));
                context.DrawRect?.Invoke(selectionRect.X, selectionRect.Y, selectionRect.Width, 2, Color.FromArgb(220, 90, 255, 120));
                context.DrawRect?.Invoke(selectionRect.X, selectionRect.Bottom - 2, selectionRect.Width, 2, Color.FromArgb(220, 90, 255, 120));
                context.DrawRect?.Invoke(selectionRect.X, selectionRect.Y, 2, selectionRect.Height, Color.FromArgb(220, 90, 255, 120));
                context.DrawRect?.Invoke(selectionRect.Right - 2, selectionRect.Y, 2, selectionRect.Height, Color.FromArgb(220, 90, 255, 120));
            }
        }

        private ButtonWidget CreateTabButton(string tabKey, string text)
        {
            return new ButtonWidget
            {
                Text = text,
                BackgroundColor = Color.FromArgb(225, 236, 236, 236),
                HoverColor = Color.FromArgb(255, 255, 255, 255),
                Brush = Brushes.Black,
                Clicked = () =>
                {
                    selectedTab = tabKey;
                    buildPageIndex = 0;
                }
            };
        }

        private List<ActorData> GetVisibleBuildActors()
        {
            return FilterActorsByTab(world.GetBuildableActors())
                .Skip(buildPageIndex * VisibleBuildItems)
                .Take(VisibleBuildItems)
                .ToList();
        }

        private int GetBuildPageCount()
        {
            return Math.Max(1, (int)Math.Ceiling(FilterActorsByTab(world.GetBuildableActors()).Count() / (float)VisibleBuildItems));
        }

        private void SelectVisibleBuildIndex(int index)
        {
            List<ActorData> visibleActors = GetVisibleBuildActors();
            if (index >= 0 && index < visibleActors.Count)
            {
                world.SelectBuildActor(visibleActors[index].TypeName);
                if (world.CanAffordSelectedBuild())
                    world.EnqueueBuild(visibleActors[index].TypeName);
            }
        }

        private IEnumerable<ActorData> FilterActorsByTab(IEnumerable<ActorData> actors)
        {
            return (actors ?? Enumerable.Empty<ActorData>()).Where(actor =>
            {
                string category = GetActorCategory(actor);
                return string.Equals(category, selectedTab, StringComparison.OrdinalIgnoreCase);
            });
        }

        private string GetActorCategory(ActorData actor)
        {
            if (actor == null)
                return TabBuilding;

            if (actor.DataField?.Properties != null &&
                actor.DataField.Properties.TryGetValue("BuildTab", out object buildTabValue))
            {
                string buildTab = buildTabValue?.ToString()?.Trim();
                if (string.Equals(buildTab, TabBuilding, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(buildTab, TabInfantry, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(buildTab, TabVehicle, StringComparison.OrdinalIgnoreCase))
                {
                    return buildTab.ToLowerInvariant();
                }
            }

            return TabBuilding;
        }

        private string BuildTooltip(ActorData actor)
        {
            if (actor == null)
                return string.Empty;

            string name = world.GetActorDisplayName(actor);
            string cost = $"Cost: {world.GetActorCost(actor.TypeName)}";
            string desc = world.GetActorDescription(actor);
            return $"{name}\n{cost}\n{desc}";
        }

        private void UpdateTabVisuals()
        {
            ApplyTabStyle(buildingTabButton, selectedTab == TabBuilding);
            ApplyTabStyle(infantryTabButton, selectedTab == TabInfantry);
            ApplyTabStyle(vehicleTabButton, selectedTab == TabVehicle);
        }

        private static void ApplyTabStyle(ButtonWidget button, bool active)
        {
            if (button == null)
                return;

            button.BackgroundColor = active
                ? Color.FromArgb(255, 255, 255, 255)
                : Color.FromArgb(225, 236, 236, 236);
            button.HoverColor = active
                ? Color.FromArgb(255, 255, 255, 255)
                : Color.FromArgb(245, 248, 248, 248);
            button.Brush = Brushes.Black;
        }

        private bool IsBuildUiHit(OpenTK.Mathematics.Vector2 mousePosition)
        {
            return buildPanel.HitTest(mousePosition) ||
                buildingTabButton.HitTest(mousePosition) ||
                infantryTabButton.HitTest(mousePosition) ||
                vehicleTabButton.HitTest(mousePosition);
        }
    }
}
