using CorrinoEngine.Core;
using CorrinoEngine.UI.Widgets;
using System.Drawing;

namespace CorrinoEngine.UI
{
    public class MainMenuScreen : UIScreen
    {
        private readonly World world;
        private readonly PanelWidget rootPanel;
        private readonly PanelWidget modalPanel;
        private readonly ButtonWidget startButton;
        private readonly ButtonWidget loadGameButton;
        private readonly ButtonWidget customBattleButton;
        private readonly ButtonWidget optionsButton;
        private readonly ButtonWidget exitButton;
        private readonly ButtonWidget prevMapButton;
        private readonly ButtonWidget nextMapButton;

        public MainMenuScreen(World world) : base("MainMenuUI")
        {
            this.world = world;
            BlocksWorldInput = true;
            rootPanel = new PanelWidget { BackgroundColor = Color.FromArgb(255, 0, 0, 0), ConsumeInput = true };
            modalPanel = new PanelWidget { BackgroundColor = Color.FromArgb(220, 12, 16, 22), ConsumeInput = true };
            startButton = new ButtonWidget
            {
                Text = "Single Player",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                    UIManager.Instance.CloseScreen(Name);
                    this.world.EnterInnerGame();
                }
            };
            loadGameButton = new ButtonWidget
            {
                Text = "Load Game",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                }
            };
            customBattleButton = new ButtonWidget
            {
                Text = "Custom Battle",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                }
            };
            optionsButton = new ButtonWidget
            {
                Text = "Options",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                }
            };
            exitButton = new ButtonWidget
            {
                Text = "Exit",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                }
            };
            prevMapButton = new ButtonWidget
            {
                Text = "<",
                BackgroundColor = Color.FromArgb(220, 38, 48, 60),
                HoverColor = Color.FromArgb(235, 64, 80, 96),
                Clicked = () => this.world.CycleMapSelection(-1)
            };
            nextMapButton = new ButtonWidget
            {
                Text = ">",
                BackgroundColor = Color.FromArgb(220, 38, 48, 60),
                HoverColor = Color.FromArgb(235, 64, 80, 96),
                Clicked = () => this.world.CycleMapSelection(1)
            };

            Root = rootPanel;
            rootPanel.AddChild(modalPanel);
            modalPanel.AddChild(prevMapButton);
            modalPanel.AddChild(nextMapButton);
            modalPanel.AddChild(startButton);
            modalPanel.AddChild(loadGameButton);
            modalPanel.AddChild(customBattleButton);
            modalPanel.AddChild(optionsButton);
            modalPanel.AddChild(exitButton);
        }

        public override void Layout(RectangleF viewport)
        {
            rootPanel.Layout(viewport);
            modalPanel.Layout(new RectangleF(viewport.Width * 0.5f - 250f, viewport.Height * 0.5f - 200f, 480f, 500f));
            prevMapButton.Layout(new RectangleF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 104f, 36f, 30f));
            nextMapButton.Layout(new RectangleF(modalPanel.Bounds.Right - 66f, modalPanel.Bounds.Y + 104f, 36f, 30f));
            startButton.Layout(new RectangleF(modalPanel.Bounds.X + 140f, modalPanel.Bounds.Bottom - 300f, 200f, 34f));
            loadGameButton.Layout(new RectangleF(modalPanel.Bounds.X + 140f, modalPanel.Bounds.Bottom - 250f, 200f, 34f));
            customBattleButton.Layout(new RectangleF(modalPanel.Bounds.X + 140f, modalPanel.Bounds.Bottom - 200f, 200f, 34f));
            optionsButton.Layout(new RectangleF(modalPanel.Bounds.X + 140f, modalPanel.Bounds.Bottom - 150f, 200f, 34f));
            exitButton.Layout(new RectangleF(modalPanel.Bounds.X + 140f, modalPanel.Bounds.Bottom - 100f, 200f, 34f));
        }

        public override void Render(UiRenderContext context)
        {
            rootPanel.Render(context);
            context.TextRenderer?.DrawString("Corrino Engine", context.TitleFont, context.WhiteBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 26f));
            context.TextRenderer?.DrawString("Select map and launch battle.", context.BodyFont, context.DimBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 64f));
            context.TextRenderer?.DrawString("Map", context.BodyFont, context.AccentBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 108f));
            context.TextRenderer?.DrawString(world.GetSelectedMapDisplayName(), context.TitleFont, context.WhiteBrush, new PointF(modalPanel.Bounds.X + 82f, modalPanel.Bounds.Y + 102f));
            context.TextRenderer?.DrawString($"Available: {world.GetAvailableMaps().Count}", context.SmallFont, context.DimBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 144f));
            prevMapButton.Render(context);
            nextMapButton.Render(context);
            startButton.Render(context);
        }
    }
}
