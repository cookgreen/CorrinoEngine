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

        public MainMenuScreen(World world) : base("MainMenuUI")
        {
            this.world = world;
            BlocksWorldInput = true;
            rootPanel = new PanelWidget { BackgroundColor = Color.FromArgb(180, 0, 0, 0), ConsumeInput = true };
            modalPanel = new PanelWidget { BackgroundColor = Color.FromArgb(220, 12, 16, 22) };
            startButton = new ButtonWidget
            {
                Text = "Start Battle",
                BackgroundColor = Color.FromArgb(220, 126, 97, 40),
                HoverColor = Color.FromArgb(235, 144, 109, 44),
                Clicked = () =>
                {
                    UIManager.Instance.CloseScreen(Name);
                    this.world.EnterInnerGame();
                }
            };

            Root = rootPanel;
            rootPanel.AddChild(modalPanel);
            modalPanel.AddChild(startButton);
        }

        public override void Layout(RectangleF viewport)
        {
            rootPanel.Layout(viewport);
            modalPanel.Layout(new RectangleF(viewport.Width * 0.5f - 180f, viewport.Height * 0.5f - 90f, 360f, 180f));
            startButton.Layout(new RectangleF(modalPanel.Bounds.X + 110f, modalPanel.Bounds.Bottom - 60f, 140f, 34f));
        }

        public override void Render(UiRenderContext context)
        {
            rootPanel.Render(context);
            context.TextRenderer?.DrawString("Corrino Engine", context.TitleFont, context.WhiteBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 26f));
            context.TextRenderer?.DrawString("Widget UI system online.", context.BodyFont, context.DimBrush, new PointF(modalPanel.Bounds.X + 30f, modalPanel.Bounds.Y + 64f));
            startButton.Render(context);
        }
    }
}
