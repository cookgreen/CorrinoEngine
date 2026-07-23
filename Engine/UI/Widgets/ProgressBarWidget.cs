using System;
using System.Drawing;

namespace CorrinoEngine.UI.Widgets
{
    public class ProgressBarWidget : Widget
    {
        public float Value01 { get; set; }
        public Color BackgroundColor { get; set; } = Color.FromArgb(120, 38, 42, 48);
        public Color FillColor { get; set; } = Color.FromArgb(220, 196, 160, 92);

        public override void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            context.DrawRect?.Invoke(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, BackgroundColor);
            float fill = Bounds.Width * Math.Clamp(Value01, 0f, 1f);
            if (fill > 0f)
                context.DrawRect?.Invoke(Bounds.X, Bounds.Y, fill, Bounds.Height, FillColor);

            base.Render(context);
        }
    }
}
