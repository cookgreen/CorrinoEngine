using System;
using System.Collections.Generic;
using System.Drawing;

namespace CorrinoEngine.UI.Widgets
{
    public class CardGridWidget<T> : Widget
    {
        private readonly List<ButtonWidget> realizedCards = new();
        private IReadOnlyList<T> items = Array.Empty<T>();

        public Func<T, string> TitleSelector { get; set; }
        public Func<T, string> SubtitleSelector { get; set; }
        public Func<T, int> IconSelector { get; set; }
        public Func<T, bool> EnabledSelector { get; set; }
        public Func<T, bool> SelectedSelector { get; set; }
        public Action<T> ItemClicked { get; set; }
        public int Columns { get; set; } = 2;
        public float CardSpacing { get; set; } = 10f;
        public float CardHeight { get; set; } = 72f;
        public Color CardColor { get; set; } = Color.FromArgb(185, 26, 33, 40);
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

            int columns = Math.Max(1, Columns);
            float cardWidth = (availableBounds.Width - CardSpacing * (columns - 1)) / columns;
            for (int i = 0; i < realizedCards.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;
                float x = availableBounds.X + column * (cardWidth + CardSpacing);
                float y = availableBounds.Y + row * (CardHeight + CardSpacing);
                realizedCards[i].Layout(new RectangleF(x, y, cardWidth, CardHeight));
            }
        }

        public override bool HandleInput(UiInputState input)
        {
            if (!Visible || !Enabled)
                return false;

            for (int i = realizedCards.Count - 1; i >= 0; i--)
            {
                if (realizedCards[i].HandleInput(input))
                    return true;
            }

            return HitTest(input.MousePosition);
        }

        public override void Render(UiRenderContext context)
        {
            if (!Visible)
                return;

            EnsureRealizedItems();
            for (int i = 0; i < realizedCards.Count; i++)
            {
                ButtonWidget card = realizedCards[i];
                if (!card.Visible)
                    continue;

                card.Render(context);
                if (i >= items.Count)
                    continue;

                T item = items[i];
                int iconTexture = IconSelector?.Invoke(item) ?? 0;
                if (iconTexture != 0)
                {
                    context.DrawTexture?.Invoke(card.Bounds.X + 8, card.Bounds.Y + 8, 40, 40, iconTexture, Color.White, 180f);
                }
                string title = TitleSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;
                string subtitle = SubtitleSelector?.Invoke(item) ?? string.Empty;
                float textX = iconTexture != 0 ? card.Bounds.X + 56 : card.Bounds.X + 8;
                context.TextRenderer?.DrawString(title, context.SmallFont ?? context.BodyFont, context.WhiteBrush, new PointF(textX, card.Bounds.Y + 10));
                if (!string.IsNullOrWhiteSpace(subtitle))
                    context.TextRenderer?.DrawString(subtitle, context.SmallFont ?? context.BodyFont, context.AccentBrush, new PointF(textX, card.Bounds.Y + 30));
            }
        }

        private void EnsureRealizedItems()
        {
            while (realizedCards.Count < items.Count)
            {
                int index = realizedCards.Count;
                ButtonWidget card = new ButtonWidget();
                card.Clicked = () =>
                {
                    if (index < items.Count)
                        ItemClicked?.Invoke(items[index]);
                };
                realizedCards.Add(card);
            }

            for (int i = 0; i < realizedCards.Count; i++)
            {
                bool visible = i < items.Count;
                ButtonWidget card = realizedCards[i];
                card.Visible = visible;
                if (!visible)
                    continue;

                T item = items[i];
                card.Text = string.Empty;
                card.Enabled = EnabledSelector?.Invoke(item) ?? true;
                card.BackgroundColor = SelectedSelector?.Invoke(item) == true ? SelectedColor : CardColor;
                card.HoverColor = HoverColor;
            }
        }
    }
}
