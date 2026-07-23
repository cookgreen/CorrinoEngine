using System.Drawing;

namespace CorrinoEngine.UI.Widgets
{
    public class LabelWidget : Widget
    {
        public string Text { get; set; } = string.Empty;
        public Font Font { get; set; }
        public Brush Brush { get; set; }
        public PointF TextPosition { get; set; }

        public override void Render(UiRenderContext context)
        {
            if (!Visible || string.IsNullOrWhiteSpace(Text))
                return;

            context.TextRenderer?.DrawString(Text, Font ?? context.BodyFont, Brush ?? context.WhiteBrush, TextPosition);
            base.Render(context);
        }
    }
}
