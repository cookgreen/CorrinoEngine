using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Mathematics;

namespace CorrinoEngine.UI.Widgets
{
    public class ListWidget<T> : Widget
    {
        private readonly List<ButtonWidget> realizedItems = new();
        private IReadOnlyList<T> items = Array.Empty<T>();

        public Func<T, string> TextSelector { get; set; }
        public Func<T, int> IconSelector { get; set; }
        public Func<T, bool> EnabledSelector { get; set; }
        public Func<T, bool> SelectedSelector { get; set; }
        public Action<T> ItemClicked { get; set; }
        public float ItemHeight { get; set; } = 28f;
        public float ItemSpacing { get; set; } = 4f;
        public Color ItemColor { get; set; } = Color.FromArgb(180, 31, 34, 39);
        public Color HoverColor { get; set; } = Color.FromArgb(205, 46, 57, 68);
        public Color SelectedColor { get; set; } = Color.FromArgb(220, 63, 79, 96);

        public void SetItems(IReadOnlyList<T> items)
        {
            this.items = items ?? Array.Empty<T>();
        }

        public override void Layout(RectangleF availableBounds)
        {
            Bounds = availableBounds;
            EnsureRealizedItems();

            float y = availableBounds.Y;
            for (int i = 0; i < realizedItems.Count; i++)
            {
                realizedItems[i].Layout(new RectangleF(availableBounds.X, y, availableBounds.Width, ItemHeight));
                y += ItemHeight + ItemSpacing;
            }
        }

        public override bool HandleInput(UiInputState input)
        {
            if (!Visible || !Enabled)
                return false;

            for (int i = realizedItems.Count - 1; i >= 0; i--)
            {
                if (realizedItems[i].HandleInput(input))
                    return true;
            }

            return HitTest(input.MousePosition);
        }

        public override void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            EnsureRealizedItems();
            for (int i = 0; i < realizedItems.Count; i++)
            {
                ButtonWidget item = realizedItems[i];
                item.Render(context);
                if (!item.Visible || i >= items.Count)
                    continue;

                int iconTexture = IconSelector?.Invoke(items[i]) ?? 0;
                if (iconTexture != 0)
                {
                    context.DrawTexture?.Invoke(item.Bounds.X + 4, item.Bounds.Y + 3, 18, 18, iconTexture, Color.White, 180f);
                }
            }
        }

        private void EnsureRealizedItems()
        {
            while (realizedItems.Count < items.Count)
            {
                int index = realizedItems.Count;
                ButtonWidget item = new ButtonWidget();
                item.Clicked = () =>
                {
                    if (index < items.Count)
                        ItemClicked?.Invoke(items[index]);
                };
                realizedItems.Add(item);
            }

            for (int i = 0; i < realizedItems.Count; i++)
            {
                bool visible = i < items.Count;
                realizedItems[i].Visible = visible;
                if (!visible)
                    continue;

                T item = items[i];
                string prefix = (IconSelector?.Invoke(item) ?? 0) != 0 ? "    " : string.Empty;
                realizedItems[i].Text = prefix + (TextSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty);
                realizedItems[i].Enabled = EnabledSelector?.Invoke(item) ?? true;
                realizedItems[i].BackgroundColor = SelectedSelector?.Invoke(item) == true ? SelectedColor : ItemColor;
                realizedItems[i].HoverColor = HoverColor;
            }
        }
    }
}
