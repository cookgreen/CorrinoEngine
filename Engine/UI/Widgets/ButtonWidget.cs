using System;
using System.Drawing;
using OpenTK.Mathematics;

namespace CorrinoEngine.UI.Widgets
{
    public class ButtonWidget : PanelWidget
    {
        public string Text { get; set; } = string.Empty;
        public Font Font { get; set; }
        public Brush Brush { get; set; }
        public Color HoverColor { get; set; } = Color.Transparent;
        public Color DisabledColor { get; set; } = Color.FromArgb(100, 45, 45, 45);
        public bool IsHovered { get; private set; }
        public Action Clicked { get; set; }

        public override bool HandleInput(UiInputState input)
        {
            if (!Visible)
                return false;

            IsHovered = HitTest(input.MousePosition);
            if (Enabled && IsHovered && input.LeftPressed)
            {
                Clicked?.Invoke();
                return true;
            }

            return base.HandleInput(input);
        }

        public override void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            Color original = BackgroundColor;
            if (!Enabled)
                BackgroundColor = DisabledColor;
            else if (IsHovered && HoverColor.A > 0)
                BackgroundColor = HoverColor;

            base.Render(context);
            BackgroundColor = original;

            if (!string.IsNullOrWhiteSpace(Text))
            {
                context.TextRenderer?.DrawString(
                    Text,
                    Font ?? context.BodyFont,
                    Brush ?? context.WhiteBrush,
                    new PointF(Bounds.X + 10, Bounds.Y + 5));
            }
        }
    }
}
