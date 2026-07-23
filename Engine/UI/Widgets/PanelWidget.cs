using System.Drawing;

namespace CorrinoEngine.UI.Widgets
{
    public class PanelWidget : Widget
    {
        public Color BackgroundColor { get; set; } = Color.Transparent;

        public override void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            if (BackgroundColor.A > 0)
                context.DrawRect?.Invoke(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, BackgroundColor);

            base.Render(context);
        }
    }
}
