using System;
using System.Drawing;

namespace CorrinoEngine.UI.Widgets
{
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }

    public class StackPanelWidget : Widget
    {
        public StackOrientation Orientation { get; set; } = StackOrientation.Vertical;
        public float Spacing { get; set; } = 4f;
        public float Padding { get; set; } = 0f;
        public float FixedItemExtent { get; set; } = 0f;

        public override void Layout(RectangleF availableBounds)
        {
            Bounds = availableBounds;

            float cursor = Padding;
            foreach (Widget child in Children)
            {
                RectangleF childBounds;
                if (Orientation == StackOrientation.Vertical)
                {
                    float height = child.Bounds.Height > 0 ? child.Bounds.Height : FixedItemExtent;
                    if (height <= 0)
                        height = Math.Max(24f, (availableBounds.Height - Padding * 2f) / Math.Max(1, Children.Count));

                    childBounds = new RectangleF(
                        availableBounds.X + Padding,
                        availableBounds.Y + cursor,
                        Math.Max(0f, availableBounds.Width - Padding * 2f),
                        height);
                    cursor += height + Spacing;
                }
                else
                {
                    float width = child.Bounds.Width > 0 ? child.Bounds.Width : FixedItemExtent;
                    if (width <= 0)
                        width = Math.Max(24f, (availableBounds.Width - Padding * 2f) / Math.Max(1, Children.Count));

                    childBounds = new RectangleF(
                        availableBounds.X + cursor,
                        availableBounds.Y + Padding,
                        width,
                        Math.Max(0f, availableBounds.Height - Padding * 2f));
                    cursor += width + Spacing;
                }

                child.Layout(childBounds);
            }
        }
    }
}
