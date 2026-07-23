using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CorrinoEngine.UI.Widgets
{
    public class CardGridWidget<T> : Widget
    {
        private readonly List<ButtonWidget> realizedCards = new();
        private IReadOnlyList<T> items = Array.Empty<T>();

        public Func<T, string> TitleSelector { get; set; }
        public Func<T, string> SubtitleSelector { get; set; }
        public Func<T, string> TooltipSelector { get; set; }
        public Func<T, int> IconSelector { get; set; }
        public Func<T, bool> EnabledSelector { get; set; }
        public Func<T, bool> SelectedSelector { get; set; }
        public Action<T> ItemClicked { get; set; }
        public int Columns { get; set; } = 2;
        public float CardSpacing { get; set; } = 10f;
        public float CardHeight { get; set; } = 72f;
        public float IconPadding { get; set; } = 8f;
        public bool DrawLabels { get; set; } = true;
        public Color CardColor { get; set; } = Color.FromArgb(185, 26, 33, 40);
        public Color HoverColor { get; set; } = Color.FromArgb(205, 46, 57, 68);
        public Color SelectedColor { get; set; } = Color.FromArgb(220, 63, 79, 96);
        public Color IconFrameColor { get; set; } = Color.FromArgb(205, 90, 110, 128);
        public Color IconHoverFrameColor { get; set; } = Color.FromArgb(240, 220, 186, 109);

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
            inputMouseX = input.MousePosition.X;
            inputMouseY = input.MousePosition.Y;

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
                float iconSize = Math.Min(card.Bounds.Width, card.Bounds.Height) - IconPadding * 2f;
                iconSize = Math.Max(24f, iconSize);
                RectangleF iconRect = new RectangleF(
                    card.Bounds.X + (card.Bounds.Width - iconSize) * 0.5f,
                    card.Bounds.Y + (card.Bounds.Height - iconSize) * 0.5f,
                    iconSize,
                    iconSize);
                bool hovered = card.IsHovered;
                if (iconTexture != 0)
                {
                    context.DrawRect?.Invoke(iconRect.X - 2, iconRect.Y - 2, iconRect.Width + 4, iconRect.Height + 4, hovered ? IconHoverFrameColor : IconFrameColor);
                    context.DrawRect?.Invoke(iconRect.X, iconRect.Y, iconRect.Width, iconRect.Height, Color.FromArgb(120, 9, 12, 16));
                    context.DrawTexture?.Invoke(iconRect.X + 4, iconRect.Y + 4, iconRect.Width - 8, iconRect.Height - 8, iconTexture, Color.White, 180f);
                }
                if (DrawLabels)
                {
                    string title = TitleSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty;
                    string subtitle = SubtitleSelector?.Invoke(item) ?? string.Empty;
                    float textX = iconTexture != 0 ? card.Bounds.X + 68 : card.Bounds.X + 10;
                    context.DrawText?.Invoke(title, context.BodyFont, context.WhiteBrush, new PointF(textX, card.Bounds.Y + 10));
                    if (!string.IsNullOrWhiteSpace(subtitle))
                        context.DrawText?.Invoke(subtitle, context.SmallFont ?? context.BodyFont, context.AccentBrush, new PointF(textX, card.Bounds.Y + 34));
                }

                if (hovered)
                {
                    string tooltip = TooltipSelector?.Invoke(item);
                    if (!string.IsNullOrWhiteSpace(tooltip))
                    {
                        Font tooltipFont = context.SmallFont ?? context.BodyFont;
                        List<string> wrappedLines = WrapTooltipLines(tooltip, tooltipFont, context, 260f);
                        float maxWidth = wrappedLines
                            .Select(line => context.MeasureText?.Invoke(line, tooltipFont).Width ?? 0f)
                            .DefaultIfEmpty(120f)
                            .Max();
                        float tooltipWidth = Math.Max(160f, maxWidth + 16f);
                        float tooltipHeight = Math.Max(26f, wrappedLines.Count * 18f + 8f);
                        float tooltipX = Math.Min(context.ViewportSize.X - tooltipWidth - 8f, inputMouseX + 20f);
                        float tooltipY = Math.Min(context.ViewportSize.Y - tooltipHeight - 8f, inputMouseY + 20f);
                        context.DrawRect?.Invoke(tooltipX, tooltipY, tooltipWidth, tooltipHeight, Color.FromArgb(228, 10, 14, 18));
                        context.DrawRect?.Invoke(tooltipX, tooltipY, tooltipWidth, 2, IconHoverFrameColor);
                        for (int lineIndex = 0; lineIndex < wrappedLines.Count; lineIndex++)
                        {
                            context.DrawText?.Invoke(wrappedLines[lineIndex], tooltipFont, context.WhiteBrush, new PointF(tooltipX + 8, tooltipY + 6 + lineIndex * 16));
                        }
                    }
                }
            }
        }

        private float inputMouseX;
        private float inputMouseY;

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

        private static List<string> WrapTooltipLines(string tooltip, Font font, UiRenderContext context, float maxWidth)
        {
            var lines = new List<string>();
            foreach (string baseLine in tooltip.Replace("\r", string.Empty).Split('\n'))
            {
                string[] words = baseLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0)
                {
                    lines.Add(string.Empty);
                    continue;
                }

                string current = words[0];
                for (int i = 1; i < words.Length; i++)
                {
                    string candidate = current + " " + words[i];
                    float candidateWidth = context.MeasureText?.Invoke(candidate, font).Width ?? 0f;
                    if (candidateWidth > maxWidth)
                    {
                        lines.Add(current);
                        current = words[i];
                    }
                    else
                    {
                        current = candidate;
                    }
                }

                lines.Add(current);
            }

            return lines;
        }
    }
}
